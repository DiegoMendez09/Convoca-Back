using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;

namespace Backend.Services.Interfaces;

public interface IMeritExamService
{
    Task<Result<IReadOnlyList<MeritExamSummaryResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<Result<MeritExamResponse>> GetAsync(Guid examId, CancellationToken cancellationToken);

    Task<Result<MeritExamResponse>> GenerateAsync(GenerateExamRequest request, CancellationToken cancellationToken);

    Task<Result<bool>> DeleteAsync(Guid examId, CancellationToken cancellationToken);
}
