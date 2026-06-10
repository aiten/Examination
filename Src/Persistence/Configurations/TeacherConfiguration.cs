using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Core.Entities;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teacher");

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.LastName, c.FirstName })
            .IsUnique();

        builder.Property(c => c.LastName)
            .HasMaxLength(128);

        builder.Property(c => c.FirstName)
            .HasMaxLength(128);

        builder.Property(c => c.NickName)
            .HasMaxLength(128);

        builder.Property(c => c.Abbreviation)
            .HasMaxLength(10);

        builder.Property(c => c.KeycloakUserId)
            .HasMaxLength(64);

        builder.HasIndex(c => c.KeycloakUserId)
            .IsUnique()
            .HasFilter("[KeycloakUserId] IS NOT NULL");
    }
}