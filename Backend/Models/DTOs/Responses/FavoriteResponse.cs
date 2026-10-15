namespace Backend.Models.DTOs.Responses;

public sealed class FavoriteResponse
{
    public Guid FavoriteId { get; set; }

    public string ProcessId { get; set; } = string.Empty;

    public string ProcedureName { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? Status { get; set; }

    public string? Modality { get; set; }

    public decimal? BasePrice { get; set; }

    public DateTime? PublishedOn { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }
}
