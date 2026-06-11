namespace Service;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;
using Persistence.QueryResult;

public interface IExamService
{
    Task<IList<Exam>> GetAllAsync();

    Task<Exam> GetByIdAsync(int id, params string[] includeProperties);

    Task UpdateExamAsync(int id, Exam exam);

    Task<Exam> AddExamAsync(Exam exam);

    Task DeleteExamAsync(int id);

    Task<IList<ExamOverview>> GetExamOverviewsAsync(int?  teacherId, int?   courseId);
    Task<StudentExam>         RegisterStudentAsync(string firstName, string lastName, string loginName, int pin);

    Task<int> CalculateGrade(int id, decimal percent);
}

public class ExamService : IExamService
{
    private readonly IUnitOfWork          _uow;
    private readonly ILogger<ExamService> _logger;

    public ExamService(IUnitOfWork uow, ILogger<ExamService> logger)
    {
        _uow    = uow;
        _logger = logger;
    }

    public async Task<IList<Exam>> GetAllAsync()
    {
        return await _uow.Exams.GetAsync(null, null, nameof(Exam.Teacher), nameof(Exam.Course));
    }

    public async Task<Exam> GetByIdAsync(int id, params string[] includeProperties)
    {
        var entity = await _uow.Exams.GetByIdAsync(id, includeProperties);
        return entity ?? throw new NotFoundException($"Exam {id} not found");
    }

    public async Task UpdateExamAsync(int id, Exam exam)
    {
        var entity = await _uow.Exams.GetByIdAsync(id);

        if (entity == null)
        {
            throw new NotFoundException($"Exam {id} not found");
        }

        entity.Description    = entity.Description;
        entity.ExamType       = entity.ExamType;
        entity.CourseId       = entity.CourseId;
        entity.TeacherId      = entity.TeacherId;
        entity.Pin            = entity.Pin;
        entity.Date           = entity.Date;
        entity.From           = entity.From;
        entity.To             = entity.To;
        entity.CanRegister    = entity.CanRegister;
        entity.CanShowResults = entity.CanShowResults;
        entity.Modified       = DateTime.Now;

        await _uow.SaveChangesAsync();
    }

    public async Task<Exam> AddExamAsync(Exam exam)
    {
        if (exam.Id != 0)
        {
            throw new NotFoundException($"Exam must not have id = 0");
        }

        exam.Created  = DateTime.Now;
        exam.Modified = null;

        await _uow.Exams.AddAsync(exam);
        await _uow.SaveChangesAsync();

        return exam;
    }

    public async Task DeleteExamAsync(int id)
    {
        var entity = await _uow.Exams.GetByIdAsync(id);

        if (entity == null)
        {
            throw new NotFoundException($"Exam {id} not found");
        }

        _uow.Exams.Remove(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId)
    {
        return await _uow.Exams.GetExamOverviewsAsync(teacherId, courseId);
    }

    public async Task<StudentExam> RegisterStudentAsync(string firstName, string lastName, string loginName, int pin)
    {
        return await _uow.Exams.RegisterStudentAsync(firstName, lastName, loginName, pin);
    }

    public Task<int> CalculateGrade(int id, decimal percent)
    {
        throw new NotImplementedException();
    }
}