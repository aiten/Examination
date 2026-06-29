namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Persistence.Model;
using Persistence.QueryResult;

using Shared;

public interface IExamRepository : IGenericRepository<Exam>
{
    Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId, int? courseYear);

    Task<Exam?> GetExamWithPINAsync(string pin);
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

    public async Task<Exam?> GetExamWithPINAsync(string pin)
    {
        return await DbSet
            .Include(e => e.Course)
            .ThenInclude(c => c.Classes)
            .FirstOrDefaultAsync(e => e.Pin == pin);
    }

    public async Task<IList<ExamOverview>> GetExamOverviewsAsync(int? teacherId, int? courseId, int? courseYear)
    {
        IQueryable<Exam> query = DbSet
            .AsNoTracking()
            .Include(e => e.Subtasks)
            .Include(e => e.StudentExams)
            .ThenInclude(se => se.Student);

        if (teacherId is not null)
        {
            _logger.LogInformation("Query with teacher={0}", teacherId);
            query = query.Where(e => e.TeacherId == teacherId);
        }

        if (courseId is not null)
        {
            _logger.LogInformation("Query with course={0}", courseId);
            query = query.Where(e => e.CourseId == courseId);
        }

        if (courseYear is not null)
        {
            _logger.LogInformation("Query with courseYear={0}", courseYear);
            query = query.Where(e => e.Course.Year == courseYear);
        }

        return await query.Select(e => new ExamOverview(
                e.Id,
                e.Description,
                e.Pin,
                $"{e.Teacher.LastName}, {e.Teacher.FirstName}",
                e.Course.Name,
                e.Course.Year,
                e.Date,
                e.From,
                e.To,
                e.Subtasks.Select(s => s.Description).ToList(),
                e.StudentExams.Select(se => StudentHelper.FullName(se.Student.FirstName,se.Student.LastName)).ToList()
            ))
            .ToListAsync();
    }
}