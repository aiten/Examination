namespace Core.Entities;

using System.Collections.Generic;

using Base.Core.Entities;

public class Teacher : EntityObject
{
    public ICollection<Exam>   Exams   { get; set; } = new List<Exam>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public string? FirstName { get; set; }

    public required string LastName { get; set; }

    public string? NickName { get; set; }

    public string? Abbreviation { get; set; }

    public string? KeycloakUserId { get; set; }
}