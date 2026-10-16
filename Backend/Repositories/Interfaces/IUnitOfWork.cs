namespace Backend.Repositories.Interfaces;

public interface IUnitOfWork
{
    IFavoriteRepository Favorites { get; }

    IProfileRepository Profiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
