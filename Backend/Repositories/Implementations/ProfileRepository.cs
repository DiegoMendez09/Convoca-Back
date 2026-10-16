using Backend.Models.Database;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public sealed class ProfileRepository : IProfileRepository
{
    public const int SingleProfileId = 1;

    private readonly ConvocaDbContext _context;

    public ProfileRepository(ConvocaDbContext context)
    {
        _context = context;
    }

    public Task<ProfessionalProfile?> GetAsync(CancellationToken cancellationToken)
    {
        return _context.ProfessionalProfiles
            .FirstOrDefaultAsync(profile => profile.ProfileId == SingleProfileId, cancellationToken);
    }

    public async Task AddAsync(ProfessionalProfile profile, CancellationToken cancellationToken)
    {
        await _context.ProfessionalProfiles.AddAsync(profile, cancellationToken);
    }
}
