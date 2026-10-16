using Backend.Models.Database;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public sealed class FunctionManualRepository : IFunctionManualRepository
{
    private readonly ConvocaDbContext _context;

    public FunctionManualRepository(ConvocaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FunctionManual>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.FunctionManuals
            .OrderByDescending(manual => manual.UploadedOn)
            .Take(40)
            .ToListAsync(cancellationToken);
    }

    public Task<FunctionManual?> GetAsync(Guid manualId, CancellationToken cancellationToken)
    {
        return _context.FunctionManuals.FirstOrDefaultAsync(manual => manual.ManualId == manualId, cancellationToken);
    }

    public async Task AddAsync(FunctionManual manual, CancellationToken cancellationToken)
    {
        await _context.FunctionManuals.AddAsync(manual, cancellationToken);
    }

    public void Remove(FunctionManual manual)
    {
        _context.FunctionManuals.Remove(manual);
    }
}
