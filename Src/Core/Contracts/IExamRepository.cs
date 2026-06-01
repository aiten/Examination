using Core.Entities;

namespace Core.Contracts;

using System.Collections.Generic;
using System.Threading.Tasks;

using Base.Core.Contracts;

using Core.QueryResult;

public interface IExamRepository : IGenericRepository<Exam>
{
    Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId);

    /// <summary>
    /// Registers a student for an exam identified by PIN.
    /// Throws <see cref="InvalidOperationException"/> when the exam or student is not found,
    /// the student is not in the exam's class, or the student is already registered.
    /// Does not call SaveChanges — the caller must commit the enclosing transaction.
    /// </summary>
    Task<StudentExam> RegisterStudentAsync(string firstName, string lastName, string loginName, int pin);

    Task<int> CalculateGrade(int id, decimal percent);
}