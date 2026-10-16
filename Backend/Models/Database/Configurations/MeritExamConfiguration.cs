using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Models.Database.Configurations;

public sealed class MeritExamConfiguration : IEntityTypeConfiguration<MeritExam>
{
    public void Configure(EntityTypeBuilder<MeritExam> builder)
    {
        builder.ToTable("MeritExams");
        builder.HasKey(exam => exam.ExamId);
        builder.Property(exam => exam.Title).IsRequired().HasMaxLength(240);
        builder.Property(exam => exam.Level).IsRequired().HasMaxLength(40);
        builder.Property(exam => exam.OpecCode).IsRequired().HasMaxLength(40);
        builder.Property(exam => exam.CallNumber).IsRequired().HasMaxLength(80);
        builder.Property(exam => exam.JobTitle).IsRequired().HasMaxLength(200);
        builder.Property(exam => exam.QuestionsJson).IsRequired();
        builder.Property(exam => exam.CreatedOn).IsRequired();
    }
}
