namespace Service;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;

using Shared.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IStudentExamService
{
    Task<StudentExamResult>          GetStudentResultAsync(string     firstName, string lastName, string pin, string registrationCode);
    Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId);
    Task<IList<StudentExamSummary>>  GetStudentExamSummaryAsync(int   examId);

    Task<StudentExam> SingleStudentExamAsync(int id, params string[] includeProperties);

    Task UpdateStudentExamAsync(int id, StudentExam value);

    Task DeleteStudentExamAsync(int id);
}

public class StudentExamService : IStudentExamService
{
    private readonly IUnitOfWork                 _uow;
    private readonly ILogger<StudentExamService> _logger;
    private readonly IHubNotificationService     _hub;

    public StudentExamService(IUnitOfWork uow, ILogger<StudentExamService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<StudentExamResult> GetStudentResultAsync(string firstName, string lastName, string pin, string registrationCode)
    {
        return await _uow.StudentExams.GetStudentResultAsync(firstName, lastName, pin, registrationCode);
    }

    public async Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId)
    {
        return await _uow.StudentExams.GetStudentExamOverviewsAsync(examId);
    }

    public async Task<IList<StudentExamSummary>> GetStudentExamSummaryAsync(int examId)
    {
        return await _uow.StudentExams.GetStudentExamSummaryAsync(examId);
    }

    public async Task<StudentExam?> GetStudentExamByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.StudentExams.GetByIdAsync(id, includeProperties);
    }

    public async Task<StudentExam> SingleStudentExamAsync(int id, params string[] includeProperties)
    {
        return (await GetStudentExamByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"StudentExam {id} not found");
    }

    public async Task UpdateStudentExamAsync(int id, StudentExam value)
    {
        var entity = await SingleStudentExamAsync(id);

        if (entity.ExamId != value.ExamId)
        {
            throw new ConflictException($"Must not change ExamId ({entity.ExamId}) for StudentExam with ID {id}");
        }

        entity.LoginName        = value.LoginName;
        entity.RegistrationCode = value.RegistrationCode;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentExamUpdatedAsync(id);
    }

    public async Task DeleteStudentExamAsync(int id)
    {
        var entity = await SingleStudentExamAsync(id, nameof(StudentExam.StudentSubtasks));

        if (entity.StudentSubtasks.Count(s => s.Result is not null) > 0)
        {
            throw new BusinessRuleException("StudentExam has results and cannot be deleted.");
        }

        await _uow.StudentExams.DeleteAsync(entity);

        await _uow.SaveChangesAsync();
        // await _hub.NotifyStudentExamUpdatedAsync(id);
    }
}