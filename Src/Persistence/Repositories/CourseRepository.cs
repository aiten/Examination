namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Persistence.Model;

public interface ICourseRepository : IGenericRepository<Course>
{
}

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}