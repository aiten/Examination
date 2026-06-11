using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Persistence.Model;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Student");

        builder.HasKey(s => s.Id);

        builder.HasIndex(c => new { c.LastName, c.FirstName })
            .IsUnique();

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasMany(s => s.Classes)
            .WithMany(c => c.Students)
            .UsingEntity(j => j.ToTable("StudentClass"));
    }
}