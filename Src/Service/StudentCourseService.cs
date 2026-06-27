namespace Service;

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;
using Persistence.Model;

using Shared.Exceptions;

public interface IStudentCourseService
{
    Task<IList<StudentCourse>> GetStudentCoursesAsync(params string[] includeProperties);

    Task<StudentCourse?> GetStudentCourseByIdAsync(int id, params string[] includeProperties);

    Task<StudentCourse> SingleStudentCourseAsync(int id, params string[] includeProperties);

    Task<IList<StudentCourse>> GetByStudentAsync(int studentId, params string[] includeProperties);

    Task<IList<StudentCourse>> GetByCourseAsync(int courseId, params string[] includeProperties);

    Task<StudentCourse> AddStudentCourseAsync(int studentId, int courseId, string? registrationCode);

    Task UpdateStudentCourseAsync(int id, string? registrationCode);

    Task DeleteStudentCourseAsync(int id);
}

public class StudentCourseService : IStudentCourseService
{
    private readonly IUnitOfWork                    _uow;
    private readonly ILogger<StudentCourseService>  _logger;

    public StudentCourseService(IUnitOfWork uow, ILogger<StudentCourseService> logger)
    {
        _uow    = uow;
        _logger = logger;
    }

    public async Task<IList<StudentCourse>> GetStudentCoursesAsync(params string[] includeProperties)
    {
        return await _uow.StudentCourses.GetAsync(null, null, includeProperties);
    }

    public async Task<StudentCourse?> GetStudentCourseByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.StudentCourses.GetByIdAsync(id, includeProperties);
    }

    public async Task<StudentCourse> SingleStudentCourseAsync(int id, params string[] includeProperties)
    {
        return (await GetStudentCourseByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"StudentCourse {id} not found");
    }

    public async Task<IList<StudentCourse>> GetByStudentAsync(int studentId, params string[] includeProperties)
    {
        return await _uow.StudentCourses.GetAsync(sc => sc.StudentId == studentId, null, includeProperties);
    }

    public async Task<IList<StudentCourse>> GetByCourseAsync(int courseId, params string[] includeProperties)
    {
        return await _uow.StudentCourses.GetAsync(sc => sc.CourseId == courseId, null, includeProperties);
    }

    public async Task<StudentCourse> AddStudentCourseAsync(int studentId, int courseId, string? registrationCode)
    {
        var entity = new StudentCourse
        {
            StudentId        = studentId,
            CourseId         = courseId,
            RegistrationCode = registrationCode
        };

        await _uow.StudentCourses.AddAsync(entity);
        await _uow.SaveChangesAsync();

        return entity;
    }

    public async Task UpdateStudentCourseAsync(int id, string? registrationCode)
    {
        var entity = await SingleStudentCourseAsync(id);

        entity.RegistrationCode = registrationCode;

        await _uow.SaveChangesAsync();
    }

    public async Task DeleteStudentCourseAsync(int id)
    {
        var entity = await SingleStudentCourseAsync(id);

        _uow.StudentCourses.Remove(entity);
        await _uow.SaveChangesAsync();
    }
}
