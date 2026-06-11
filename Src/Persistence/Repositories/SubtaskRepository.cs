using Core.Contracts;
using Core.Entities;

using Base.Persistence;

namespace Persistence;

public class SubtaskRepository : GenericRepository<Subtask>, ISubtaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SubtaskRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}