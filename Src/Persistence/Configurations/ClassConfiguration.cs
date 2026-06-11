using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Persistence.Model;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Class");

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.Description, c.Year })
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(32);

        builder.HasOne(j => j.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}