namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface IClassRepository : IGenericRepository<Class>
{
    Task<Class?> GetClassByNameAndYearAsync(string description, int year);
}

public class ClassRepository : GenericRepository<Class>, IClassRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClassRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Class?> GetClassByNameAndYearAsync(string className, int year)
    {
        return await _dbContext.Classes
            .FirstOrDefaultAsync(c => c.Description == className && c.Year == year);
    }
}