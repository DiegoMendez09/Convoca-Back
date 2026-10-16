namespace Backend.Repositories.Interfaces;

public interface IUnitOfWork
{
    IFavoriteRepository Favorites { get; }

    IProfileRepository Profiles { get; }

    IMeritExamRepository Exams { get; }

    IFunctionManualRepository Manuals { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
