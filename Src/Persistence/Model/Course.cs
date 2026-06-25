namespace Persistence.Model;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Base.Persistence.Model;

public class Course : EntityObject
{
    public required string Name { get; set; }

    public int Year { get; set; }

    [Range(10000, 99999)]
    public int? Pin { get; set; }

    public bool CanRegister { get; set; } = true;

    public Subject Subject   { get; set; } = null!;
    public int     SubjectId { get; set; }

    public ICollection<Class>         Classes        { get; set; } = new List<Class>();
    public ICollection<Teacher>       Teachers       { get; set; } = new List<Teacher>();
    public ICollection<Exam>          Exams          { get; set; } = new List<Exam>();
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}