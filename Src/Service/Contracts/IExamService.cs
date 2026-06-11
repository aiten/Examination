namespace Service.Contracts;

using System.Collections.Generic;
using System.Threading.Tasks;

using Persistence.Model;
using Persistence.QueryResult;

public interface IExamService
{
    Task<IList<ExamOverview>> GetExamOverviewsAsync(int?  teacherId, int?   courseId);
    Task<StudentExam>         RegisterStudentAsync(string firstName, string lastName, string loginName, int pin);

    Task<int> CalculateGrade(int id, decimal percent);
}