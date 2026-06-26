namespace Persistence.QueryResult;

using System;
using System.Collections.Generic;

public record ExamOverview(
    int                 Id,
    string              Description,
    string?             Pin,
    string              Teacher,
    string              Course,
    int                 CourseYear,
    DateOnly            Date,
    TimeOnly            From,
    TimeOnly            To,
    ICollection<string> Subtask,
    ICollection<string> Students
);