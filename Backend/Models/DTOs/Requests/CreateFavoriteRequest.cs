namespace Backend.Models.DTOs.Requests;

public sealed class CreateFavoriteRequest
{
    public string ProcessId { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
