using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;

namespace Backend.Services.Interfaces;

public interface IFunctionManualService
{
    Task<Result<IReadOnlyList<FunctionManualResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<Result<FunctionManualResponse>> UploadAsync(Stream content, string fileName, string opecCode, CancellationToken cancellationToken);

    Task<Result<bool>> DeleteAsync(Guid manualId, CancellationToken cancellationToken);
}
