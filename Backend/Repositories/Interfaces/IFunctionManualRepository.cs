using Backend.Models.Database;

namespace Backend.Repositories.Interfaces;

public interface IFunctionManualRepository
{
    Task<IReadOnlyList<FunctionManual>> ListAsync(CancellationToken cancellationToken);

    Task<FunctionManual?> GetAsync(Guid manualId, CancellationToken cancellationToken);

    Task AddAsync(FunctionManual manual, CancellationToken cancellationToken);

    void Remove(FunctionManual manual);
}
