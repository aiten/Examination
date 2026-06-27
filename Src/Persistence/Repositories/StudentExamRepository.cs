namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;
using Persistence.QueryResult;

using Shared;
using Shared.Exceptions;

public interface IStudentExamRepository : IGenericRepository<StudentExam>
{
    Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId);
    Task<IList<StudentExamSummary>>  GetStudentExamSummaryAsync(int   examId);

    Task<StudentExam?> GetStudentExamAsync(int examId, string firstName, string lastName, string registrationCode);

    Task DeleteAsync(StudentExam entity);

    Task<bool> AnyAsync(int examId, int    studentId);
    Task<bool> AnyAsync(int examId, string registrationCode);

    void Check(int examId, int studentExamId);
}

public class StudentExamRepository : GenericRepository<StudentExam>, IStudentExamRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentExamRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AnyAsync(int examId, int studentId)
    {
        return await _dbContext.StudentExams
            .AnyAsync(se => se.ExamId == examId && se.StudentId == studentId);
    }

    public async Task<bool> AnyAsync(int examId, string registrationCode)
    {
        return await _dbContext.StudentExams
            .AnyAsync(se => se.ExamId == examId && se.RegistrationCode == registrationCode);
    }

    public async Task DeleteAsync(StudentExam entity)
    {
        _dbContext.StudentSubtasks.RemoveRange(entity.StudentSubtasks);
        Remove(entity);
    }

    public async Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId)
    {
        var subtasks = await _dbContext.Subtasks
            .Where(s => s.ExamId == examId)
            .ToListAsync();

        var totalMaxPoints = subtasks.Sum(s => s.Bonus ? 0 : s.Points);
        var countRatable   = subtasks.Count(s => s.Bonus == false);

        var result = await _dbContext.StudentExams
            .Where(se => se.ExamId == examId)
            .Select(se => new
            {
                se.Id,
                se.StudentId,
                se.Student.FirstName,
                se.Student.LastName,
                se.LoginName,
                se.RegistrationCode,
                Points     = (decimal)se.StudentSubtasks.Sum(ss => (double)(ss.Result ?? 0m) * (double)ss.Subtask.Points),
                CountRated = se.StudentSubtasks.Count(ss => ss.Result.HasValue && ss.Subtask.Bonus == false)
            })
            .ToListAsync();

        return result.Select(r => new StudentExamOverview(
            r.Id,
            r.StudentId,
            r.FirstName,
            r.LastName,
            r.LoginName,
            r.RegistrationCode,
            r.CountRated,
            r.CountRated == countRatable ? r.Points : null,
            totalMaxPoints != 0 && r.CountRated == countRatable ? Math.Round(r.Points / totalMaxPoints * 100m, 2) : null,
            totalMaxPoints != 0 && r.CountRated == countRatable ? ExamHelper.CalculateGrade(r.Points / totalMaxPoints) : null
        )).ToList();
    }

    public async Task<IList<StudentExamSummary>> GetStudentExamSummaryAsync(int examId)
    {
        var studenResults = await GetStudentExamOverviewsAsync(examId);
        return studenResults
            .GroupBy(r => r.Grade)
            .Select(g => new StudentExamSummary(g.Key, g.Count()))
            .ToList();
    }

    public async Task<StudentExam?> GetStudentExamAsync(int examId, string firstName, string lastName, string registrationCode)
    {
        return await _dbContext.StudentExams
            .Include(se => se.Student)
            .Include(se => se.StudentSubtasks)
            .ThenInclude(ss => ss.Subtask)
            .FirstOrDefaultAsync(se =>
                se.ExamId == examId &&
                se.RegistrationCode == registrationCode &&
                se.Student.FirstName == firstName &&
                se.Student.LastName == lastName);

    }

    public void Check(int examId, int studentExamId)
    {
        throw new NotImplementedException();
    }
}