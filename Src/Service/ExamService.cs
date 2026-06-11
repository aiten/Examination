namespace Service;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;

using Service.Contracts;

public class ExamService : IExamService
{
    private readonly IUnitOfWork          _uow;
    private readonly ILogger<ExamService> _logger;

    public ExamService(IUnitOfWork uow, ILogger<ExamService> logger)
    {
        _uow    = uow;
        _logger = logger;
    }

    public Task<int> CalculateGrade(int id, decimal percent)
    {
        throw new NotImplementedException();
    }

    public Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId)
    {
        throw new NotImplementedException();
    }

    public Task<StudentExam> RegisterStudentAsync(string firstName, string lastName, string loginName, int pin)
    {
        throw new NotImplementedException();
    }
}