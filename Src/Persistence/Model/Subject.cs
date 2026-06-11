namespace Core.Entities;

using System.Collections.Generic;

using Base.Core.Entities;

public class Subject : EntityObject
{
    public required string Name { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
