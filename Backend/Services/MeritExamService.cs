using System.Text.Json;
using Backend.Models.Database;
using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services;

public sealed class MeritExamService : IMeritExamService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IUnitOfWork? _unitOfWork;

    public MeritExamService(IServiceProvider serviceProvider)
    {
        _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
    }

    public async Task<Result<IReadOnlyList<MeritExamSummaryResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Result<IReadOnlyList<MeritExamSummaryResponse>>.Ok([]);

        var exams = await _unitOfWork.Exams.ListAsync(cancellationToken);
        var summaries = exams.Select(exam => new MeritExamSummaryResponse
        {
            ExamId = exam.ExamId,
            Title = exam.Title,
            Level = exam.Level,
            OpecCode = exam.OpecCode,
            QuestionCount = CountQuestions(exam.QuestionsJson),
            CreatedOn = exam.CreatedOn
        }).ToArray();

        return Result<IReadOnlyList<MeritExamSummaryResponse>>.Ok(summaries);
    }

    public async Task<Result<MeritExamResponse>> GetAsync(Guid examId, CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable<MeritExamResponse>();

        var exam = await _unitOfWork.Exams.GetAsync(examId, cancellationToken);
        if (exam is null)
            return Result<MeritExamResponse>.Fail("No se encontró el examen.", StatusCodes.Status404NotFound);

        return Result<MeritExamResponse>.Ok(ToResponse(exam, saved: true, notice: null));
    }

    public async Task<Result<MeritExamResponse>> GenerateAsync(GenerateExamRequest request, CancellationToken cancellationToken)
    {
        var level = MeritExamGenerator.NormalizeLevel(request.Level);
        if (level is null)
            return Result<MeritExamResponse>.Fail("El nivel debe ser Asistencial, Técnico, Profesional o Asesor.", StatusCodes.Status400BadRequest);

        var opec = (request.OpecCode ?? string.Empty).Trim();
        var job = (request.JobTitle ?? string.Empty).Trim();
        var call = (request.CallNumber ?? string.Empty).Trim();
        var entity = (request.EntityName ?? string.Empty).Trim();
        var purpose = (request.Purpose ?? string.Empty).Trim();
        var functions = (request.Functions ?? string.Empty).Trim();
        var knowledge = (request.Knowledge ?? string.Empty).Trim();
        var axes = (request.Axes ?? string.Empty).Trim();
        var grade = (request.Grade ?? string.Empty).Trim();

        if (request.ManualId is Guid manualId)
        {
            if (_unitOfWork is null)
                return Unavailable<MeritExamResponse>();

            var manual = await _unitOfWork.Manuals.GetAsync(manualId, cancellationToken);
            if (manual is null)
                return Result<MeritExamResponse>.Fail("No se encontró el manual de esa OPEC.", StatusCodes.Status404NotFound);

            opec = manual.OpecCode;
            job = manual.JobTitle;
            purpose = manual.Purpose;
            functions = manual.Functions;
            knowledge = manual.Knowledge;
            if (!string.IsNullOrWhiteSpace(manual.Level))
                level = manual.Level;
            if (!string.IsNullOrWhiteSpace(manual.Grade))
                grade = manual.Grade;
        }

        if (opec.Length is 0 or > 40)
            return Result<MeritExamResponse>.Fail("Escribe el código de la OPEC, de hasta 40 caracteres.", StatusCodes.Status400BadRequest);

        if (job.Length is 0 or > 200)
            return Result<MeritExamResponse>.Fail("Escribe la denominación del empleo, de hasta 200 caracteres.", StatusCodes.Status400BadRequest);

        if (call.Length > 80 || entity.Length > 200)
            return Result<MeritExamResponse>.Fail("La convocatoria o la entidad superan el tamaño permitido.", StatusCodes.Status400BadRequest);

        if (purpose.Length > 8000 || functions.Length > 8000 || knowledge.Length > 8000 || axes.Length > 1000)
            return Result<MeritExamResponse>.Fail("El manual supera el tamaño permitido.", StatusCodes.Status400BadRequest);

        if (purpose.Length + functions.Length + knowledge.Length < 40)
            return Result<MeritExamResponse>.Fail("Sube el manual y la OPEC, o pega el propósito, las funciones o los conocimientos.", StatusCodes.Status400BadRequest);

        var drafts = MeritExamGenerator.Build(new MeritExamSource(level, call, opec, job, purpose, functions, knowledge, axes));
        if (drafts.Count == 0)
            return Result<MeritExamResponse>.Fail("No hubo material suficiente para armar el examen.", StatusCodes.Status400BadRequest);

        var questions = drafts.Select((draft, index) => new ExamQuestionResponse
        {
            Number = index + 1,
            Section = draft.Section,
            Topic = draft.Topic,
            Stem = draft.Stem,
            Options = draft.Options,
            CorrectIndex = draft.CorrectIndex
        }).ToArray();

        var gradeLabel = string.IsNullOrWhiteSpace(grade) ? string.Empty : $" grado {grade}";
        var title = $"{job}{gradeLabel} · OPEC {opec}";
        var response = new MeritExamResponse
        {
            Title = title,
            Level = level,
            OpecCode = opec,
            CallNumber = call,
            JobTitle = job,
            Questions = questions,
            CreatedOn = DateTime.UtcNow.AddHours(-5)
        };

        if (_unitOfWork is null)
        {
            response.Notice = "La base de Azure no está configurada. El examen se generó, pero no quedó guardado.";
            return Result<MeritExamResponse>.Ok(response);
        }

        var exam = new MeritExam
        {
            ExamId = Guid.NewGuid(),
            Title = title,
            Level = level,
            OpecCode = opec,
            CallNumber = call,
            JobTitle = job,
            QuestionsJson = JsonSerializer.Serialize(questions, JsonOptions),
            CreatedOn = response.CreatedOn.Value
        };

        await _unitOfWork.Exams.AddAsync(exam, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        response.ExamId = exam.ExamId;
        response.Saved = true;
        return Result<MeritExamResponse>.Ok(response);
    }

    public async Task<Result<bool>> DeleteAsync(Guid examId, CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable<bool>();

        var exam = await _unitOfWork.Exams.GetAsync(examId, cancellationToken);
        if (exam is null)
            return Result<bool>.Fail("No se encontró el examen.", StatusCodes.Status404NotFound);

        _unitOfWork.Exams.Remove(exam);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }

    private static MeritExamResponse ToResponse(MeritExam exam, bool saved, string? notice)
    {
        var questions = JsonSerializer.Deserialize<List<ExamQuestionResponse>>(exam.QuestionsJson, JsonOptions) ?? [];
        return new MeritExamResponse
        {
            ExamId = exam.ExamId,
            Title = exam.Title,
            Level = exam.Level,
            OpecCode = exam.OpecCode,
            CallNumber = exam.CallNumber,
            JobTitle = exam.JobTitle,
            Saved = saved,
            Notice = notice,
            CreatedOn = exam.CreatedOn,
            Questions = questions
        };
    }

    private static int CountQuestions(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<ExamQuestionResponse>>(json, JsonOptions)?.Count ?? 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    private static Result<T> Unavailable<T>()
    {
        return Result<T>.Fail(
            "La base de Azure no está configurada. El examen no se puede consultar.",
            StatusCodes.Status503ServiceUnavailable);
    }
}
