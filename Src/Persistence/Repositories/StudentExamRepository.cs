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
    Task<StudentExam?> GetStudentExamAsync(int examId, string firstName, string lastName, string registrationCode);

    Task<StudentExam?> GetStudentExamAsync(int examId, int studentId);

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

    public async Task<StudentExam?> GetStudentExamAsync(int examId, int studentId)
    {
        return await _dbContext.StudentExams
            .Include(se => se.Student)
            .Include(se => se.StudentSubtasks)
            .ThenInclude(ss => ss.Subtask)
            .FirstOrDefaultAsync(se =>
                se.ExamId == examId &&
                se.StudentId == studentId);
    }

    public void Check(int examId, int studentExamId)
    {
        throw new NotImplementedException();
    }
}