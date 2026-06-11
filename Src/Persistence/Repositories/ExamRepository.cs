namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Persistence.Model;
using Persistence.QueryResult;

public interface IExamRepository : IGenericRepository<Exam>
{
    Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId);

    Task<Exam?> GetExamWithPINAsync(int pin);

    Task<int> CalculateGrade(int id, decimal percent);
}

public class ExamRepository : GenericRepository<Exam>, IExamRepository
{
    private readonly ApplicationDbContext    _dbContext;
    private readonly ILogger<ExamRepository> _logger;

    public ExamRepository(ApplicationDbContext dbContext, ILogger<ExamRepository> logger) : base(dbContext)
    {
        _dbContext = dbContext;
        _logger    = logger;
    }

    public async Task<Exam?> GetExamWithPINAsync(int pin)
    {
        return await DbSet
            .Include(e => e.Course)
            .ThenInclude(c => c.Classes)
            .FirstOrDefaultAsync(e => e.Pin == pin);
    }

    public async Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId)
    {
        var query = _dbContext.Exams.AsNoTracking();

        if (teacherId is not null)
        {
            _logger.LogInformation("Query with teacher={0}", teacherId);
            query = query.Where(j => j.TeacherId == teacherId);
        }

        if (courseId is not null)
        {
            _logger.LogInformation("Query with course={0}", courseId);
            query = query.Where(j => j.CourseId == courseId);
        }

        return await query.Select(e => new ExamOverview(
                e.Id,
                e.Description,
                e.Pin,
                $"{e.Teacher.LastName}, {e.Teacher.FirstName}",
                e.Course.Name,
                e.Date,
                e.From,
                e.To,
                e.Subtasks.Select(s => s.Description).ToList(),
                e.StudentExams.Select(se => $"{se.Student.LastName}, {se.Student.FirstName}").ToList()
            ))
            .ToListAsync();
    }

    public async Task<int> CalculateGrade(int id, decimal percent)
    {
        return CalculateGrade(percent);
    }

    public static int CalculateGrade(decimal percent)
    {
        return percent switch
        {
            >= 0.88m => 1,
            >= 0.75m => 2,
            >= 0.63m => 3,
            >= 0.5m  => 4,
            _        => 5
        };
    }
}