namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface IStudentCourseRepository : IGenericRepository<StudentCourse>
{
    Task<StudentCourse?> GetByStudentAndCourseAsync(int studentId, int courseId);

    Task<bool> AnyAsync(int studentId, int    courseId);
    Task<bool> AnyAsync(int courseId,  string registrationCode);
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

    public async Task<bool> AnyAsync(int studentId, int courseId)
    {
        return await DbSet
            .AnyAsync(se => se.CourseId == courseId && se.StudentId == studentId);
    }

    public async Task<bool> AnyAsync(int courseId, string registrationCode)
    {
        return await DbSet
            .AnyAsync(se => se.CourseId == courseId && se.RegistrationCode == registrationCode);
    }
}