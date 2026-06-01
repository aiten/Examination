using Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Teacher>        Teachers        { get; set; }
    public DbSet<Subtask>        Subtasks        { get; set; }
    public DbSet<Exam>           Exams           { get; set; }
    public DbSet<Class>          Classes         { get; set; }
    public DbSet<Student>        Students        { get; set; }
    public DbSet<StudentExam>    StudentExams    { get; set; }
    public DbSet<StudentSubtask> StudentSubtasks { get; set; }
    public DbSet<Subject>        Subjects        { get; set; }
    public DbSet<Course>         Courses         { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}