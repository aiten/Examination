namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Model;

public class Class : EntityObject
{
    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public required string Description { get; set; }

    public int Year { get; set; }

    public Teacher? Teacher   { get; set; } = null!;
    public int?     TeacherId { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}