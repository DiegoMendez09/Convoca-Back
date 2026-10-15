using Backend.Models.Database;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly SecopDbContext _context;

    public FavoriteRepository(SecopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Favorite>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Favorites
            .AsNoTracking()
            .OrderByDescending(favorite => favorite.CreatedOn)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<Favorite?> GetByProcessIdAsync(string processId, CancellationToken cancellationToken)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(favorite => favorite.ProcessId == processId, cancellationToken);
    }

    public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken)
    {
        await _context.Favorites.AddAsync(favorite, cancellationToken);
    }

    public void Delete(Favorite favorite)
    {
        _context.Favorites.Remove(favorite);
    }
}
