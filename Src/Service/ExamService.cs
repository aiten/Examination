using Core.Contracts;
using Core.Entities;

namespace Persistence;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Base.Persistence;

using Core.QueryResult;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ExamService : IExamService
{
    private readonly IUnitOfWork          _uow;
    private readonly ILogger<ExamService> _logger;

    public ExamService(IUnitOfWork uow, ILogger<ExamService> logger)
    {
        _uow = uow;
        _logger    = logger;
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