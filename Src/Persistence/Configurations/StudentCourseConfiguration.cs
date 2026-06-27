using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Persistence.Model;

public class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> builder)
    {
        builder.ToTable(nameof(StudentCourse));

        builder.HasKey(sc => sc.Id);

        builder.HasIndex(sc => new { sc.StudentId, sc.CourseId })
            .IsUnique();

        builder.Property(sc => sc.RegistrationCode)
            .HasMaxLength(5);

        builder.HasIndex(se => new { se.RegistrationCode, se.CourseId })
            .IsUnique();

        builder.HasOne(sc => sc.Student)
            .WithMany()
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.Course)
            .WithMany(c => c.StudentCourses)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
