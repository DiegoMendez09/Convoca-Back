using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Models.DTOs.Responses.Pagination;

namespace Backend.Services.Interfaces;

public interface IProcessService
{
    Task<Result<PaginationResponse<ProcessResponse>>> SearchAsync(
        ProcessSearchRequest request,
        CancellationToken cancellationToken);

    Task<Result<ProcessResponse>> GetByIdAsync(string processId, CancellationToken cancellationToken);

    Task<Result<ProcessExportFile>> ExportAsync(
        ProcessSearchRequest request,
        CancellationToken cancellationToken);
}
