namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface ISubtaskRepository : IGenericRepository<Subtask>
{
    Task<IList<Subtask>> GetForExamAsync(int examId);
}

public class SubtaskRepository : GenericRepository<Subtask>, ISubtaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SubtaskRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<Subtask>> GetForExamAsync(int examId)
    {
        return await DbSet
            .Where(s => s.ExamId == examId)
            .ToListAsync();
    }
}