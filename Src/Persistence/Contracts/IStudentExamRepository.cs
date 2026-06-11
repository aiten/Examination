using System.Collections.Generic;
using System.Threading.Tasks;

using Core.Entities;
using Core.QueryResult;

namespace Core.Contracts;

using Base.Core.Contracts;

public interface IStudentExamRepository : IGenericRepository<StudentExam>
{
    Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId);
    Task<IList<StudentExamSummary>> GetStudentExamSummaryAsync(int examId);
    Task<StudentExamResult> GetStudentResultAsync(string firstName, string lastName, int pin, string registrationCode);

    Task DeleteAsync(int studentExamId);

    void Check(int examId, int studentExamId);
}