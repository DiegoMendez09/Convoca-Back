using Backend.Models.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models.Database;

public sealed class SecopDbContext : DbContext
{
    public SecopDbContext(DbContextOptions<SecopDbContext> options) : base(options)
    {
    }

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<ProfessionalProfile> ProfessionalProfiles => Set<ProfessionalProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FavoriteConfiguration());
        modelBuilder.ApplyConfiguration(new ProfessionalProfileConfiguration());
    }
}
