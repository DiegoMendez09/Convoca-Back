namespace Backend.Models.Database;

public class MeritExam
{
    public Guid ExamId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string OpecCode { get; set; } = string.Empty;

    public string CallNumber { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string QuestionsJson { get; set; } = "[]";

    public DateTime CreatedOn { get; set; }
}
