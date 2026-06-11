namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Entities;

public class Course : EntityObject
{
    public required string Name { get; set; }

    public int Year { get; set; }

    public Subject Subject   { get; set; } = null!;
    public int     SubjectId { get; set; }

    public ICollection<Class>   Classes  { get; set; } = new List<Class>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public ICollection<Exam>    Exams    { get; set; } = new List<Exam>();
}