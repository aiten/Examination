namespace Persistence.Model;

using Base.Persistence.Model;

public class StudentCourse : EntityObject
{
    public string? RegistrationCode { get; set; }

    public Student Student   { get; set; } = null!;
    public int     StudentId { get; set; }

    public Course Course   { get; set; } = null!;
    public int    CourseId { get; set; }
}
