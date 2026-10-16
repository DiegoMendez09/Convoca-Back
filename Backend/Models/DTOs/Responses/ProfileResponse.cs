namespace Backend.Models.DTOs.Responses;

public sealed class ProfileResponse
{
    public string Title { get; set; } = string.Empty;

    public string Experience { get; set; } = string.Empty;

    public string Keywords { get; set; } = string.Empty;

    public bool UseForSearch { get; set; }

    public string? SearchText { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
