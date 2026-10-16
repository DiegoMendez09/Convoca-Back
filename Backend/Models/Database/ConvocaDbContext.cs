using Backend.Models.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models.Database;

public sealed class ConvocaDbContext : DbContext
{
    public ConvocaDbContext(DbContextOptions<ConvocaDbContext> options) : base(options)
    {
    }

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<ProfessionalProfile> ProfessionalProfiles => Set<ProfessionalProfile>();

    public DbSet<MeritExam> MeritExams => Set<MeritExam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FavoriteConfiguration());
        modelBuilder.ApplyConfiguration(new ProfessionalProfileConfiguration());
        modelBuilder.ApplyConfiguration(new MeritExamConfiguration());
    }
}
