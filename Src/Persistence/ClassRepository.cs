using Core.Contracts;
using Core.Entities;

using Base.Persistence;

namespace Persistence;

public class ClassRepository : GenericRepository<Class>, IClassRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClassRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}