namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface IStudentCourseRepository : IGenericRepository<StudentCourse>
{
    Task<StudentCourse?> GetByStudentAndCourseAsync(int studentId, int courseId);
}

public class StudentCourseRepository : GenericRepository<StudentCourse>, IStudentCourseRepository
{
    public StudentCourseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<StudentCourse?> GetByStudentAndCourseAsync(int studentId, int courseId)
    {
        return await DbSet.SingleOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);
    }
}
