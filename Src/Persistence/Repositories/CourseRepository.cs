namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<Course?> GetCourseWithPINAsync(string pin, bool includeClasses = false, bool includeExams=false);
}

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Course?> GetCourseWithPINAsync(string pin, bool includeClasses = false, bool includeExams = false)
    {
        var query = DbSet
            .AsTracking();

        if (includeClasses)
        {
            query = query.Include(c => c.Classes);
        }

        if (includeExams)
        {
            query = query.Include(c => c.Exams);
        }

        return await query
            .FirstOrDefaultAsync(e => e.Pin == pin);
    }
}