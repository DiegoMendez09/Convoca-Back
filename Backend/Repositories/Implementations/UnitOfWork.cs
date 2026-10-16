using Backend.Models.Database;
using Backend.Repositories.Interfaces;

namespace Backend.Repositories.Implementations;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ConvocaDbContext _context;
    private IFavoriteRepository? _favorites;
    private IProfileRepository? _profiles;
    private IMeritExamRepository? _exams;

    public UnitOfWork(ConvocaDbContext context)
    {
        _context = context;
    }

    public IFavoriteRepository Favorites => _favorites ??= new FavoriteRepository(_context);

    public IProfileRepository Profiles => _profiles ??= new ProfileRepository(_context);

    public IMeritExamRepository Exams => _exams ??= new MeritExamRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
