namespace Backend.Repositories.Interfaces;

public interface IUnitOfWork
{
    IFavoriteRepository Favorites { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
