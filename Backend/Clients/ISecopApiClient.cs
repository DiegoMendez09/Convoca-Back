using Backend.Models.External;

namespace Backend.Clients;

public interface ISecopApiClient
{
    Task<IReadOnlyList<SecopProcessRecord>> SearchAsync(
        string where,
        string? fullText,
        int limit,
        int offset,
        bool compact,
        CancellationToken cancellationToken);

    Task<int> CountAsync(string where, CancellationToken cancellationToken);
}
