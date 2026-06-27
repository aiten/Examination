using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Persistence.Model;

public class StudentExamConfiguration : IEntityTypeConfiguration<StudentExam>
{
    public void Configure(EntityTypeBuilder<StudentExam> builder)
    {
        builder.ToTable(nameof(StudentExam));

        builder.HasKey(se => se.Id);

        builder.Property(se => se.LoginName)
            .HasMaxLength(32);

        builder.Property(se => se.RegistrationCode)
            .HasMaxLength(5);

        builder.Property(se => se.Comment)
            .HasMaxLength(256);

        builder.HasIndex(se => new { se.RegistrationCode, se.ExamId })
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