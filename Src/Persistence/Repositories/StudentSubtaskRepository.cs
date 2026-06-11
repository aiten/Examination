using Core.Contracts;
using Core.Entities;

using Base.Persistence;

namespace Persistence;

using Microsoft.EntityFrameworkCore;

public class StudentSubtaskRepository : GenericRepository<StudentSubtask>, IStudentSubtaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentSubtaskRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<StudentSubtask>> GetAllForStudentAsync(int examId, int studentExamId)
    {
        var subtasksPossible = await _dbContext
            .Subtasks
            .AsNoTracking()
            .Where(st => st.ExamId == examId)
            .Select(st => new StudentSubtask()
            {
                Result = null,
                StudentExamId = studentExamId,
                SubtaskId = st.Id,
                Subtask = st

            })
            .ToListAsync();

        var listInDb = await _dbContext
            .StudentSubtasks
            .AsNoTracking()
            .Include(ss => ss.Subtask)
            .Where(ss => ss.StudentExamId == studentExamId)
            .ToListAsync();

        subtasksPossible = subtasksPossible.ExceptBy(listInDb.Select(ss => ss.SubtaskId), s => s.SubtaskId).ToList();
        listInDb.AddRange(subtasksPossible);

        return listInDb;
    }

    public async Task<IList<StudentSubtask>> GetAllForSubtaskAsync(int examId, int subtaskId)
    {
        var allStudentExams = await _dbContext
            .StudentExams
            .AsNoTracking()
            .Include(se => se.Student)
            .Where(se => se.ExamId == examId)
            .ToListAsync();

        var existingResults = await _dbContext
            .StudentSubtasks
            .AsNoTracking()
            .Include(ss => ss.StudentExam)
            .ThenInclude(se => se.Student)
            .Where(ss => ss.SubtaskId == subtaskId && ss.StudentExam.ExamId == examId)
            .ToListAsync();

        var notYetGraded = allStudentExams
            .Where(se => !existingResults.Any(ss => ss.StudentExamId == se.Id))
            .Select(se => new StudentSubtask
            {
                Result        = null,
                StudentExamId = se.Id,
                StudentExam   = se,
                SubtaskId     = subtaskId,
            })
            .ToList();

        existingResults.AddRange(notYetGraded);
        return existingResults;
    }
}