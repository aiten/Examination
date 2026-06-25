namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Model;

public class Subject : EntityObject
{
    public required string Name { get; set; }

    public string? Comment { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}