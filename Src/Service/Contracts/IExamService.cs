using Core.Entities;

namespace Core.Contracts;

using System.Collections.Generic;
using System.Threading.Tasks;

using Base.Core.Contracts;

using Core.QueryResult;

public interface IExamService
{
    Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId);
    Task<StudentExam> RegisterStudentAsync(string firstName, string lastName, string loginName, int pin);

    Task<int> CalculateGrade(int id, decimal percent);
}