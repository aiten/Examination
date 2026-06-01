using Core.Contracts;
using Core.Entities;

namespace Persistence;

using Base.Persistence;

public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
