using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Models.Database.Configurations;

public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");
        builder.HasKey(favorite => favorite.FavoriteId);

        builder.Property(favorite => favorite.ProcessId).IsRequired().HasMaxLength(80);
        builder.HasIndex(favorite => favorite.ProcessId).IsUnique();

        builder.Property(favorite => favorite.ProcedureName).IsRequired().HasMaxLength(500);
        builder.Property(favorite => favorite.EntityName).IsRequired().HasMaxLength(300);
        builder.Property(favorite => favorite.Status).HasMaxLength(80);
        builder.Property(favorite => favorite.Modality).HasMaxLength(200);
        builder.Property(favorite => favorite.BasePrice).HasPrecision(18, 2);
        builder.Property(favorite => favorite.Notes).HasMaxLength(500);
        builder.Property(favorite => favorite.CreatedOn).IsRequired();
    }
}
