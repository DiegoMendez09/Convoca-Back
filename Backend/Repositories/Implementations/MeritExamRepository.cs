using Backend.Models.Database;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public sealed class MeritExamRepository : IMeritExamRepository
{
    private readonly ConvocaDbContext _context;

    public MeritExamRepository(ConvocaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MeritExam>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.MeritExams
            .OrderByDescending(exam => exam.CreatedOn)
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    public Task<MeritExam?> GetAsync(Guid examId, CancellationToken cancellationToken)
    {
        return _context.MeritExams.FirstOrDefaultAsync(exam => exam.ExamId == examId, cancellationToken);
    }

    public async Task AddAsync(MeritExam exam, CancellationToken cancellationToken)
    {
        await _context.MeritExams.AddAsync(exam, cancellationToken);
    }

    public void Remove(MeritExam exam)
    {
        _context.MeritExams.Remove(exam);
    }
}
