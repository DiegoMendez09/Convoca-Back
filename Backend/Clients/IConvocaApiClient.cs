using Backend.Models.External;

namespace Backend.Clients;

public interface IConvocaApiClient
{
    Task<IReadOnlyList<ConvocaProcessRecord>> SearchAsync(
        string where,
        string? fullText,
        int limit,
        int offset,
        bool compact,
        CancellationToken cancellationToken);

    Task<int> CountAsync(string where, CancellationToken cancellationToken);
}
