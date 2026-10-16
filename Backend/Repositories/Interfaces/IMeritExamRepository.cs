using Backend.Models.Database;

namespace Backend.Repositories.Interfaces;

public interface IMeritExamRepository
{
    Task<IReadOnlyList<MeritExam>> ListAsync(CancellationToken cancellationToken);

    Task<MeritExam?> GetAsync(Guid examId, CancellationToken cancellationToken);

    Task AddAsync(MeritExam exam, CancellationToken cancellationToken);

    void Remove(MeritExam exam);
}
