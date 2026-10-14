namespace Backend.Models.DTOs.Responses;

public sealed class ProcessResponse
{
    public string ProcessId { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public string? EntityNit { get; set; }

    public string? Department { get; set; }

    public string? City { get; set; }

    public string ProcedureName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Phase { get; set; }

    public DateTime? PublishedOn { get; set; }

    public DateTime? LastPublishedOn { get; set; }

    public DateTime? OffersDueOn { get; set; }

    public decimal? BasePrice { get; set; }

    public string? Modality { get; set; }

    public int? Duration { get; set; }

    public string? DurationUnit { get; set; }

    public string? Status { get; set; }

    public string? Awarded { get; set; }

    public DateTime? AwardedOn { get; set; }

    public decimal? AwardAmount { get; set; }

    public string? SupplierName { get; set; }

    public string? SupplierNit { get; set; }

    public string? UnspscCode { get; set; }

    public string? OpeningStatus { get; set; }

    public string? ContractType { get; set; }

    public string? ProcessUrl { get; set; }
}
