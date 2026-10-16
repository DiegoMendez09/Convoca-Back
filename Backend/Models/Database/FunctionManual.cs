namespace Backend.Models.Database;

public class FunctionManual
{
    public Guid ManualId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string OpecCode { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public string Functions { get; set; } = string.Empty;

    public string Knowledge { get; set; } = string.Empty;

    public string SectionText { get; set; } = string.Empty;

    public DateTime UploadedOn { get; set; }
}
