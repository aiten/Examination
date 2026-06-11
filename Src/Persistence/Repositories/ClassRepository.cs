namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Persistence.Model;

public interface IClassRepository : IGenericRepository<Class>
{
}

public class ClassRepository : GenericRepository<Class>, IClassRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClassRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}