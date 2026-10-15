using Backend.Models.Database;

namespace Backend.Repositories.Interfaces;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<Favorite>> GetAllAsync(CancellationToken cancellationToken);

    Task<Favorite?> GetByProcessIdAsync(string processId, CancellationToken cancellationToken);

    Task AddAsync(Favorite favorite, CancellationToken cancellationToken);

    void Delete(Favorite favorite);
}
