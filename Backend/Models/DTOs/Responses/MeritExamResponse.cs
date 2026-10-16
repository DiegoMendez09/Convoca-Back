namespace Backend.Models.DTOs.Responses;

public sealed class MeritExamResponse
{
    public Guid? ExamId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string OpecCode { get; set; } = string.Empty;

    public string CallNumber { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public bool Saved { get; set; }

    public string? Notice { get; set; }

    public DateTime? CreatedOn { get; set; }

    public IReadOnlyList<ExamQuestionResponse> Questions { get; set; } = [];
}

public sealed class ExamQuestionResponse
{
    public int Number { get; set; }

    public string Section { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public string Stem { get; set; } = string.Empty;

    public IReadOnlyList<string> Options { get; set; } = [];

    public int CorrectIndex { get; set; }
}

public sealed class MeritExamSummaryResponse
{
    public Guid ExamId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string OpecCode { get; set; } = string.Empty;

    public int QuestionCount { get; set; }

    public DateTime CreatedOn { get; set; }
}
