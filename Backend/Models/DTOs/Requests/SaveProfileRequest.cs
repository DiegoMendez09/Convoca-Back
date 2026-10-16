namespace Backend.Models.DTOs.Requests;

public sealed class SaveProfileRequest
{
    public string? Title { get; set; }

    public string? Experience { get; set; }

    public string? Keywords { get; set; }

    public bool UseForSearch { get; set; }
}
