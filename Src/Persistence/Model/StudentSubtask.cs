namespace Persistence.Model;

using Base.Persistence.Entities;

public class StudentSubtask : EntityObject
{
    public decimal? Result { get; set; }

    public string? Comment { get; set; }

    public string? CommentPrivate { get; set; }

    public StudentExam StudentExam   { get; set; } = null!;
    public int         StudentExamId { get; set; }

    public Subtask Subtask   { get; set; } = null!;
    public int     SubtaskId { get; set; }
}