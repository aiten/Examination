using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

using Base.Persistence;

using Persistence.Model;

public class StudentSubtaskConfiguration : IEntityTypeConfiguration<StudentSubtask>
{
    public void Configure(EntityTypeBuilder<StudentSubtask> builder)
    {
        builder.ToTable("StudentSubtask");

        builder.HasKey(ss => ss.Id);

        builder.HasIndex(c => new { c.StudentExamId, c.SubtaskId })
            .IsUnique();

        builder.Property(ss => ss.Comment)
            .HasMaxLength(250);

        builder.Property(ss => ss.CommentPrivate)
            .HasMaxLength(250);

        builder.Property(ss => ss.Result)
            .AsDecimal(3, 2);

        builder.HasOne(ss => ss.StudentExam)
            .WithMany(se => se.StudentSubtasks)
            .HasForeignKey(ss => ss.StudentExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ss => ss.Subtask)
            .WithMany(s => s.StudentSubtasks)
            .HasForeignKey(ss => ss.SubtaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}