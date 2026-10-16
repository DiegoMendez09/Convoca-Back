using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Models.Database.Configurations;

public sealed class ProfessionalProfileConfiguration : IEntityTypeConfiguration<ProfessionalProfile>
{
    public void Configure(EntityTypeBuilder<ProfessionalProfile> builder)
    {
        builder.ToTable("ProfessionalProfiles");
        builder.HasKey(profile => profile.ProfileId);

        builder.Property(profile => profile.Title).IsRequired().HasMaxLength(200);
        builder.Property(profile => profile.Experience).IsRequired().HasMaxLength(2000);
        builder.Property(profile => profile.Keywords).IsRequired().HasMaxLength(300);
        builder.Property(profile => profile.UseForSearch).IsRequired();
        builder.Property(profile => profile.UpdatedOn).IsRequired();
    }
}
