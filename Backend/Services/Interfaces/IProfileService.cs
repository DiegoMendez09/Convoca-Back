using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;

namespace Backend.Services.Interfaces;

public interface IProfileService
{
    Task<Result<ProfileResponse>> GetAsync(CancellationToken cancellationToken);

    Task<Result<ProfileResponse>> SaveAsync(SaveProfileRequest request, CancellationToken cancellationToken);
}
