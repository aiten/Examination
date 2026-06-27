using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Persistence.Model;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable(nameof(Course));

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(c => c.CanShowResults)
            .HasDefaultValue(false);

        builder.HasOne(c => c.Subject)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Classes)
            .WithMany(cl => cl.Courses);

        builder.HasMany(c => c.Teachers)
            .WithMany(t => t.Courses);
    }
}