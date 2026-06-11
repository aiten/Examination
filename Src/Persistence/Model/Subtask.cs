namespace Persistence.Model;

using System.Collections.Generic;

using Base.Persistence.Entities;

public class Subtask : EntityObject
{
    public required string Description { get; set; }

    public int SeqNo { get; set; }

    public int  Points { get; set; }
    public bool Bonus  { get; set; }

    public int ExamId { get; set; }

    public Exam Exam { get; set; } = null!;

    public ICollection<StudentSubtask> StudentSubtasks { get; set; } = new List<StudentSubtask>();
}