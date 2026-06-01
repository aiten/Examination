namespace Core.Contracts;

using Base.Core.Contracts;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IExamRepository           Exams           { get; }
    ITeacherRepository        Teachers        { get; }
    IClassRepository          Classes         { get; }
    ISubtaskRepository        Subtasks        { get; }
    IStudentRepository        Students        { get; }
    IStudentExamRepository    StudentExams    { get; }
    IStudentSubtaskRepository StudentSubtasks { get; }
    ISubjectRepository        Subjects        { get; }
    ICourseRepository         Courses         { get; }
}