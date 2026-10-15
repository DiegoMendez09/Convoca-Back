using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;

namespace Backend.Services.Interfaces;

public interface IFavoriteService
{
    Task<Result<IReadOnlyList<FavoriteResponse>>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<FavoriteResponse>> CreateAsync(CreateFavoriteRequest request, CancellationToken cancellationToken);

    Task<Result<bool>> DeleteAsync(string processId, CancellationToken cancellationToken);
}
