namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Persistence.Model;

public interface ISubtaskRepository : IGenericRepository<Subtask>
{
}

public class SubtaskRepository : GenericRepository<Subtask>, ISubtaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SubtaskRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}