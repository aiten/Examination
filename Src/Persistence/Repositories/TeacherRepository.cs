namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Persistence.Model;

public interface ITeacherRepository : IGenericRepository<Teacher>
{
}

public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TeacherRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}