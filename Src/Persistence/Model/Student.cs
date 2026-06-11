namespace Core.Entities;

using Base.Core.Entities;

using System.Collections.Generic;

public class Student : EntityObject
{
    public required string FirstName { get; set; }
    public required string LastName  { get; set; }

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}