namespace Backend.Models.DTOs.Requests;

public sealed class GenerateExamRequest
{
    public string? CallNumber { get; set; }

    public string? EntityName { get; set; }

    public string? Level { get; set; }

    public string? OpecCode { get; set; }

    public string? JobTitle { get; set; }

    public string? Grade { get; set; }

    public string? Purpose { get; set; }

    public string? Functions { get; set; }

    public string? Knowledge { get; set; }

    public string? Axes { get; set; }
}
