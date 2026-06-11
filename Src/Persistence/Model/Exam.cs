using Core.Validations;

namespace Core.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Base.Core.Entities;

public class Exam : EntityObject
{
    public required string Description { get; set; }

    public ExamType ExamType { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly From { get; set; }

    [ExamRange]
    public TimeOnly To { get; set; }

    [Range(10000, 99999)]
    public int? Pin { get; set; }

    public bool CanRegister    { get; set; } = true;
    public bool CanShowResults { get; set; } = false;

    public DateTime  Created  { get; set; }
    public DateTime? Modified { get; set; } = null;

    public Teacher Teacher   { get; set; } = null!;
    public int     TeacherId { get; set; }

    public Course Course   { get; set; } = null!;
    public int    CourseId { get; set; }

    public ICollection<Subtask> Subtasks { get; set; } = null!;

    public ICollection<StudentExam> StudentExams { get; set; } = null!;
}