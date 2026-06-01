using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Core.Entities;

public class StudentExamConfiguration : IEntityTypeConfiguration<StudentExam>
{
    public void Configure(EntityTypeBuilder<StudentExam> builder)
    {
        builder.ToTable("StudentExam");

        builder.HasKey(se => se.Id);

        builder.Property(se => se.LoginName)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(se => se.RegistrationCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.HasIndex(se => new {se.RegistrationCode, se.ExamId})
            .IsUnique();

        builder.HasIndex(se => new { se.StudentId, se.ExamId })
            .IsUnique();

        builder.HasOne(se => se.Student)
            .WithMany()
            .HasForeignKey(se => se.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(se => se.Exam)
            .WithMany(s => s.StudentExams)
            .HasForeignKey(se => se.ExamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}