using Backend.Models.Database;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services;

public sealed class FunctionManualService : IFunctionManualService
{
    private readonly IUnitOfWork? _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public FunctionManualService(IServiceProvider serviceProvider, IWebHostEnvironment environment)
    {
        _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        _environment = environment;
    }

    public async Task<Result<IReadOnlyList<FunctionManualResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Result<IReadOnlyList<FunctionManualResponse>>.Ok([]);

        var manuals = await _unitOfWork.Manuals.ListAsync(cancellationToken);
        return Result<IReadOnlyList<FunctionManualResponse>>.Ok(manuals.Select(ToResponse).ToArray());
    }

    public async Task<Result<FunctionManualResponse>> UploadAsync(
        Stream content,
        string fileName,
        string opecCode,
        CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
        {
            return Result<FunctionManualResponse>.Fail(
                "La base de Azure no está configurada. El manual no se puede guardar.",
                StatusCodes.Status503ServiceUnavailable);
        }

        var code = ManualOpecExtractor.NormalizeCode(opecCode);
        if (code.Length is < 3 or > 40)
            return Result<FunctionManualResponse>.Fail("Escribe el código de la OPEC que quieres extraer.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return Result<FunctionManualResponse>.Fail("El manual debe ser un archivo PDF.", StatusCodes.Status400BadRequest);

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length == 0)
            return Result<FunctionManualResponse>.Fail("El PDF está vacío.", StatusCodes.Status400BadRequest);

        buffer.Position = 0;
        string text;
        try
        {
            text = ManualPdfText.Read(buffer);
        }
        catch (Exception)
        {
            return Result<FunctionManualResponse>.Fail("No se pudo leer el PDF.", StatusCodes.Status400BadRequest);
        }

        var section = ManualOpecExtractor.Extract(text, code);
        if (section is null)
        {
            return Result<FunctionManualResponse>.Fail(
                "Esa OPEC no está en el manual. No se guardó nada.",
                StatusCodes.Status400BadRequest);
        }

        var manualId = Guid.NewGuid();
        var directory = Path.Combine(_environment.ContentRootPath, "App_Data", "manuals");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, manualId + ".pdf");

        buffer.Position = 0;
        await using (var file = File.Create(path))
            await buffer.CopyToAsync(file, cancellationToken);

        var manual = new FunctionManual
        {
            ManualId = manualId,
            OriginalFileName = Cut(Path.GetFileName(fileName), 260),
            OpecCode = section.OpecCode,
            JobTitle = Cut(section.JobTitle, 200),
            Level = Cut(section.Level, 40),
            Grade = Cut(section.Grade, 20),
            Purpose = section.Purpose,
            Functions = section.Functions,
            Knowledge = section.Knowledge,
            SectionText = section.SectionText,
            UploadedOn = DateTime.UtcNow.AddHours(-5)
        };

        try
        {
            await _unitOfWork.Manuals.AddAsync(manual, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(path))
                File.Delete(path);
            throw;
        }

        return Result<FunctionManualResponse>.Ok(ToResponse(manual));
    }

    public async Task<Result<bool>> DeleteAsync(Guid manualId, CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
        {
            return Result<bool>.Fail(
                "La base de Azure no está configurada. El manual no se puede eliminar.",
                StatusCodes.Status503ServiceUnavailable);
        }

        var manual = await _unitOfWork.Manuals.GetAsync(manualId, cancellationToken);
        if (manual is null)
            return Result<bool>.Fail("No se encontró el manual.", StatusCodes.Status404NotFound);

        var path = Path.Combine(_environment.ContentRootPath, "App_Data", "manuals", manualId + ".pdf");
        _unitOfWork.Manuals.Remove(manual);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (File.Exists(path))
            File.Delete(path);

        return Result<bool>.Ok(true);
    }

    internal static string ManualPath(string contentRoot, Guid manualId)
    {
        return Path.Combine(contentRoot, "App_Data", "manuals", manualId + ".pdf");
    }

    private static string Cut(string? value, int max)
    {
        var text = (value ?? string.Empty).Trim();
        return text.Length <= max ? text : text[..max].Trim();
    }

    private static FunctionManualResponse ToResponse(FunctionManual manual)
    {
        return new FunctionManualResponse
        {
            ManualId = manual.ManualId,
            FileName = manual.OriginalFileName,
            OpecCode = manual.OpecCode,
            JobTitle = manual.JobTitle,
            Level = manual.Level,
            Grade = manual.Grade,
            Purpose = manual.Purpose,
            Functions = manual.Functions,
            Knowledge = manual.Knowledge,
            UploadedOn = manual.UploadedOn
        };
    }
}
