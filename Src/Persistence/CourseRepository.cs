using Core.Contracts;
using Core.Entities;

namespace Persistence;

using Base.Persistence;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
