using Backend.Models.Database;
using Backend.Repositories.Interfaces;

namespace Backend.Repositories.Implementations;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SecopDbContext _context;
    private IFavoriteRepository? _favorites;

    public UnitOfWork(SecopDbContext context)
    {
        _context = context;
    }

    public IFavoriteRepository Favorites => _favorites ??= new FavoriteRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
