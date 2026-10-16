using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Models.Database.Configurations;

public sealed class FunctionManualConfiguration : IEntityTypeConfiguration<FunctionManual>
{
    public void Configure(EntityTypeBuilder<FunctionManual> builder)
    {
        builder.ToTable("FunctionManuals");
        builder.HasKey(manual => manual.ManualId);
        builder.Property(manual => manual.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(manual => manual.OpecCode).IsRequired().HasMaxLength(40);
        builder.Property(manual => manual.JobTitle).IsRequired().HasMaxLength(200);
        builder.Property(manual => manual.Level).IsRequired().HasMaxLength(40);
        builder.Property(manual => manual.Grade).IsRequired().HasMaxLength(20);
        builder.Property(manual => manual.Purpose).IsRequired();
        builder.Property(manual => manual.Functions).IsRequired();
        builder.Property(manual => manual.Knowledge).IsRequired();
        builder.Property(manual => manual.SectionText).IsRequired();
        builder.Property(manual => manual.UploadedOn).IsRequired();
    }
}
