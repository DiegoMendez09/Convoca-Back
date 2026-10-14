using Backend.Models.External;

namespace Backend.Clients;

public interface ISecopApiClient
{
    Task<IReadOnlyList<SecopProcessRecord>> SearchAsync(
        string where,
        int limit,
        int offset,
        CancellationToken cancellationToken);

    Task<int> CountAsync(string where, CancellationToken cancellationToken);
}
