using Backend.Models.Database;

namespace Backend.Repositories.Interfaces;

public interface IProfileRepository
{
    Task<ProfessionalProfile?> GetAsync(CancellationToken cancellationToken);

    Task AddAsync(ProfessionalProfile profile, CancellationToken cancellationToken);
}
