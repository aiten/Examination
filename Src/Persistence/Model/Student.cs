namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Model;

using Microsoft.EntityFrameworkCore.Metadata;

using Shared;

public class Student : EntityObject
{
    public required string FirstName { get; set; }
    public required string LastName  { get; set; }

    public ICollection<Class>         Classes        { get; set; } = new List<Class>();

    public string FullName => StudentHelper.FullName(FirstName,LastName);
}