namespace Persistence.QueryResult;

using System;
using System.Collections.Generic;

public record StudentCourseResult(
    string                          CourseName,
    string                          StudentName,
    IList<StudentExamResult> StudentExams
);