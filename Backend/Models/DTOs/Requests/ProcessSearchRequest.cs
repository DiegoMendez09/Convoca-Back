namespace Backend.Models.DTOs.Requests;

public sealed class ProcessSearchRequest
{
    public string? Keyword { get; set; }

    public string? EntityName { get; set; }

    public string? EntityNit { get; set; }

    public string? Department { get; set; }

    public string? City { get; set; }

    public string? Modality { get; set; }

    public string? Phase { get; set; }

    public string? Status { get; set; }

    public string? ContractType { get; set; }

    public DateTime? PublishedFrom { get; set; }

    public DateTime? PublishedTo { get; set; }

    public decimal? MinBasePrice { get; set; }

    public decimal? MaxBasePrice { get; set; }

    public bool? PublishedOnly { get; set; }

    public bool? OpenForOffer { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
