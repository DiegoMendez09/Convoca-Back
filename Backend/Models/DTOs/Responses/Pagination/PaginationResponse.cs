namespace Backend.Models.DTOs.Responses.Pagination;

public sealed class PaginationResponse<T>
{
    public List<T> Items { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public bool HasMore { get; set; }
}
