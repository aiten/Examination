namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Entities;

public class StudentExam : EntityObject
{
    public required string LoginName        { get; set; }
    public required string RegistrationCode { get; set; }

    public Student Student   { get; set; } = null!;
    public int     StudentId { get; set; }

    public Exam Exam   { get; set; } = null!;
    public int  ExamId { get; set; }

    public ICollection<StudentSubtask> StudentSubtasks { get; set; } = new List<StudentSubtask>();
}