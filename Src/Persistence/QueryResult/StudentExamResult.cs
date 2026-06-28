namespace Persistence.QueryResult;

using System;
using System.Collections.Generic;

// The result, which can be displayed to the student, of an exam.
// It contains the description and date of the exam, the name of the student, a list of subtasks with their description, points, result, comment and whether they are bonus or not, and the total points, percent and grade for the exam.

public record StudentExamResultSubtask(
    int      SeqNo,
    string   Description,
    int      Points,
    decimal? Result,
    string?  Comment,
    bool     Bonus
);

public record StudentExamResult(
    string?                         Status,
    string                          ExamDescription,
    DateOnly?                       ExamDate,
    string                          StudentName,
    IList<StudentExamResultSubtask> Subtasks,
    decimal?                        TotalPoints,
    decimal?                        Percent,
    int?                            Grade
);