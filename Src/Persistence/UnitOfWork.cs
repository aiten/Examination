namespace Persistence;

using Base.Persistence;
using Base.Persistence.Contracts;

using Persistence.Repositories;

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

public class UnitOfWork : BaseUnitOfWork, IUnitOfWork
{
    public UnitOfWork(ApplicationDbContext dBContext,
        IClassRepository                   classes,
        ITeacherRepository                 teachers,
        ISubtaskRepository                 subtasks,
        IExamRepository                    exams,
        IStudentRepository                 students,
        IStudentExamRepository             studentExams,
        IStudentSubtaskRepository          studentSubtasks,
        ISubjectRepository                 subjects,
        ICourseRepository                  courses
    ) : base(dBContext)
    {
        Subtasks        = subtasks;
        Teachers        = teachers;
        Classes         = classes;
        Exams           = exams;
        Students        = students;
        StudentExams    = studentExams;
        StudentSubtasks = studentSubtasks;
        Subjects        = subjects;
        Courses         = courses;
    }

    public IClassRepository          Classes         { get; }
    public ITeacherRepository        Teachers        { get; }
    public ISubtaskRepository        Subtasks        { get; }
    public IExamRepository           Exams           { get; }
    public IStudentRepository        Students        { get; }
    public IStudentExamRepository    StudentExams    { get; }
    public IStudentSubtaskRepository StudentSubtasks { get; }
    public ISubjectRepository        Subjects        { get; }
    public ICourseRepository         Courses         { get; }
}