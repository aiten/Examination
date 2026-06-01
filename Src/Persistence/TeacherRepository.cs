using Core.Contracts;
using Core.Entities;

using Base.Persistence;

namespace Persistence;

public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TeacherRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}