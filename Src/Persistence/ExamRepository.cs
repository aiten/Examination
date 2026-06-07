using Core.Contracts;
using Core.Entities;

namespace Persistence;

using System.Diagnostics;

using Base.Persistence;

using Core.QueryResult;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ExamRepository : GenericRepository<Exam>, IExamRepository
{
    private readonly ApplicationDbContext    _dbContext;
    private readonly ILogger<ExamRepository> _logger;

    public ExamRepository(ApplicationDbContext dbContext, ILogger<ExamRepository> logger) : base(dbContext)
    {
        _dbContext = dbContext;
        _logger    = logger;
    }

    public async Task<StudentExam> RegisterStudentAsync(string firstName, string lastName, string loginName, int pin)
    {
        var exam = await _dbContext.Exams.FirstOrDefaultAsync(e => e.Pin == pin);
        if (exam is null)
            throw new InvalidOperationException($"No exam found with PIN {pin}");

        var student = await _dbContext.Students
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.FirstName == firstName && s.LastName == lastName);
        if (student is null)
            throw new InvalidOperationException($"No student found with name '{lastName}, {firstName}'");

        var course = await _dbContext.Courses
            .Include(c => c.Classes)
            .FirstOrDefaultAsync(c => c.Id == exam.CourseId);
        if (course is null || !course.Classes.Any(c => student.Classes.Any(sc => sc.Id == c.Id)))
            throw new InvalidOperationException($"Student '{lastName}, {firstName} ' is not enrolled in any class of this exam's course");

        bool alreadyRegistered = await _dbContext.StudentExams
            .AnyAsync(se => se.StudentId == student.Id && se.ExamId == exam.Id);
        if (alreadyRegistered)
            throw new InvalidOperationException($"Student '{lastName}, {firstName}' is already registered for this exam");

        var registration = new StudentExam
        {
            StudentId        = student.Id,
            ExamId           = exam.Id,
            LoginName        = loginName,
            RegistrationCode = await GenerateUniqueRegistrationCodeAsync(exam.Id),
            Student          = student,
            Exam             = exam
        };

        await _dbContext.StudentExams.AddAsync(registration);
        return registration;
    }

    private async Task<string> GenerateUniqueRegistrationCodeAsync(int examId)
    {
        var    rng = Random.Shared;
        string code;
        do
        {
            code = rng.Next(10000, 100000).ToString();
        } while (await _dbContext.StudentExams.AnyAsync(se => se.ExamId == examId && se.RegistrationCode == code));

        return code;
    }

    public async Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId)
    {
        var query = _dbContext.Exams.AsNoTracking();

        if (teacherId is not null)
        {
            _logger.LogInformation("Query with teacher={0}", teacherId);
            query = query.Where(j => j.TeacherId == teacherId);
        }

        if (courseId is not null)
        {
            _logger.LogInformation("Query with course={0}", courseId);
            query = query.Where(j => j.CourseId == courseId);
        }

        return await query.Select(e => new ExamOverview(
                e.Id,
                e.Description,
                e.Pin,
                $"{e.Teacher.LastName}, {e.Teacher.FirstName}",
                e.Course.Name,
                e.Date,
                e.From,
                e.To,
                e.Subtasks.Select(s => s.Description).ToList(),
                e.StudentExams.Select(se => $"{se.Student.LastName}, {se.Student.FirstName}").ToList()
            ))
            .ToListAsync();
    }

    public async Task<int> CalculateGrade(int id, decimal percent)
    {
        return CalculateGrade(percent);
    }

    public static int CalculateGrade(decimal percent)
    {
        return percent switch
        {
            >= 0.88m => 1,
            >= 0.75m => 2,
            >= 0.63m => 3,
            >= 0.5m  => 4,
            _        => 5
        };
    }
}