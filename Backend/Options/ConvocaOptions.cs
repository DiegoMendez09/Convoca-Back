namespace Backend.Options;

public sealed class ConvocaOptions
{
    public const string SectionName = "Convoca";

    public string BaseUrl { get; set; } = string.Empty;

    public string DatasetId { get; set; } = string.Empty;

    public string AppToken { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}
