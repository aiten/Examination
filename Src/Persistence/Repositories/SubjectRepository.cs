namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Persistence.Model;

public interface ISubjectRepository : IGenericRepository<Subject>
{
}

public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}