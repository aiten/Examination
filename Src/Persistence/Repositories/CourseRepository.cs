namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<Course?> GetCourseWithPINAsync(string pin);
}

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Course?> GetCourseWithPINAsync(string pin)
    {
        return await DbSet
            .Include(e => e.Classes)
            .FirstOrDefaultAsync(e => e.Pin == pin);
    }
}