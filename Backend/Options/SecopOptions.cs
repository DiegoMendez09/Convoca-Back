namespace Backend.Options;

public sealed class SecopOptions
{
    public const string SectionName = "Secop";

    public string BaseUrl { get; set; } = string.Empty;

    public string DatasetId { get; set; } = string.Empty;

    public string AppToken { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}
