namespace Backend.Repositories.Interfaces;

public interface IUnitOfWork
{
    IFavoriteRepository Favorites { get; }

    IProfileRepository Profiles { get; }

    IMeritExamRepository Exams { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
