namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;
using Persistence.QueryResult;

public interface IExamService
{
    Task<IList<Exam>> GetExamsAsync();

    Task<Exam?>  GetExamByIdAsync(int     id, params string[] includeProperties);
    
    Task<Exam> SingleExamAsync(int id, params string[] includeProperties);

    Task UpdateExamAsync(int id, Exam exam);

    Task<Exam> AddExamAsync(Exam exam);

    Task DeleteExamAsync(int id);

    Task<IList<ExamOverview>> GetExamOverviewsAsync(int?  teacherId, int?   courseId);

    Task<StudentExam>         RegisterStudentAsync(string firstName, string lastName, string? loginName, int pin);
}

public class ExamService : IExamService
{
    private readonly IUnitOfWork              _uow;
    private readonly ILogger<ExamService>     _logger;
    private readonly IHubNotificationService  _hub;

    public ExamService(IUnitOfWork uow, ILogger<ExamService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Exam>> GetExamsAsync()
    {
        return await _uow.Exams.GetAsync(null, null, nameof(Exam.Teacher), nameof(Exam.Course));
    }

    public async Task<Exam?> GetExamByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Exams.GetByIdAsync(id, includeProperties);
    }
    public async Task<Exam> SingleExamAsync(int id, params string[] includeProperties)
    {
        return await GetExamByIdAsync(id, includeProperties) ?? throw new NotFoundException($"Exam {id} not found"); 
    }

    public async Task UpdateExamAsync(int id, Exam exam)
    {
        var entity = await SingleExamAsync(id);

        entity.Description    = exam.Description;
        entity.ExamType       = exam.ExamType;
        entity.CourseId       = exam.CourseId;
        entity.TeacherId      = exam.TeacherId;
        entity.Pin            = exam.Pin;
        entity.Date           = exam.Date;
        entity.From           = exam.From;
        entity.To             = exam.To;
        entity.CanRegister    = exam.CanRegister;
        entity.CanShowResults = exam.CanShowResults;
        entity.Modified       = DateTime.Now;

        await _uow.SaveChangesAsync();
        await _hub.NotifyExamUpdatedAsync(id);
    }

    public async Task<Exam> AddExamAsync(Exam exam)
    {
        if (exam.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        exam.Created  = DateTime.Now;
        exam.Modified = null;

        await _uow.Exams.AddAsync(exam);
        await _uow.SaveChangesAsync();
        await _hub.NotifyExamUpdatedAsync(exam.Id);

        return exam;
    }

    public async Task DeleteExamAsync(int id)
    {
        var entity = await SingleExamAsync(id);

        _uow.Exams.Remove(entity);
        await _uow.SaveChangesAsync();
        await _hub.NotifyExamUpdatedAsync(id);
    }

    public async Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId)
    {
        return await _uow.Exams.GetExamOverviewsAsync(teacherId, courseId);
    }

    public async Task<StudentExam> RegisterStudentAsync(string firstName, string lastName, string? loginName, int pin)
    {
        var exam = await _uow.Exams.GetExamWithPINAsync(pin);
        if (exam is null)
            throw new IllegalValuesException($"No exam found with PIN {pin}");

        var student = await _uow.Students.GetStudentByNameAsync(lastName, firstName);
        if (student is null)
            throw new IllegalValuesException($"No student found with name '{lastName}, {firstName}'");

        var course = exam.Course;
        if (course is null || !course.Classes.Any(c => student.Classes.Any(sc => sc.Id == c.Id)))
            throw new IllegalValuesException($"Student '{lastName}, {firstName} ' is not enrolled in any class of this exam's course");

        bool alreadyRegistered = await _uow.StudentExams.AnyAsync(exam.Id, student.Id);
        if (alreadyRegistered)
            throw new IllegalValuesException($"Student '{lastName}, {firstName}' is already registered for this exam");

        var registration = new StudentExam
        {
            StudentId        = student.Id,
            ExamId           = exam.Id,
            LoginName        = string.IsNullOrEmpty(loginName) ? null : loginName,
            RegistrationCode = await GenerateUniqueRegistrationCodeAsync(exam.Id),
            Student          = student,
            Exam             = exam
        };

        await _uow.StudentExams.AddAsync(registration);

        // check, if a StudentCourse exists and created

        var studentCourse = await _uow.StudentCourses.GetByStudentAndCourseAsync(student.Id, course.Id);

        if (studentCourse is null)
        {
            studentCourse = new StudentCourse()
            {
                Course  = course,
                Student = student,
            };
            await _uow.StudentCourses.AddAsync(studentCourse);
        }

        await _uow.SaveChangesAsync();

        return registration;
    }

    private async Task<string> GenerateUniqueRegistrationCodeAsync(int examId)
    {
        var    rng = Random.Shared;
        string code;
        do
        {
            code = rng.Next(10000, 100000).ToString();
        } while (await _uow.StudentExams.AnyAsync(examId, code));

        return code;
    }
}