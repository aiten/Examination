namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Model;

public class Student : EntityObject
{
    public required string FirstName { get; set; }
    public required string LastName  { get; set; }

    public ICollection<Class>         Classes        { get; set; } = new List<Class>();
}