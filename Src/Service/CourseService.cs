namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface ICourseService
{
    Task<IList<Course>> GetCoursesAsync(params string[] includeProperties);

    Task<Course?> GetCourseByIdAsync(int id, params string[] includeProperties);

    Task<Course> SingleCourseAsync(int id, params string[] includeProperties);

    Task UpdateCourseAsync(int id, Course value);
    Task UpdateCourseAsync(int id, string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds);

    Task<Course> AddCourseAsync(Course value);
    Task<Course> AddCourseAsync(string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds);

    Task DeleteCourseAsync(int id);
}

public class CourseService : ICourseService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<CourseService>  _logger;
    private readonly IHubNotificationService _hub;

    public CourseService(IUnitOfWork uow, ILogger<CourseService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Course>> GetCoursesAsync(params string[] includeProperties)
    {
        return await _uow.Courses.GetAsync(null, null, includeProperties);
    }

    public async Task<Course?> GetCourseByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Courses.GetByIdAsync(id, includeProperties);
    }

    public async Task<Course> SingleCourseAsync(int id, params string[] includeProperties)
    {
        return (await GetCourseByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"Course {id} not found");
    }

    public async Task UpdateCourseAsync(int id, Course value)
    {
        var entity = await SingleCourseAsync(id);

        entity.Name      = value.Name;
        entity.Year      = value.Year;
        entity.SubjectId = value.SubjectId;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyCourseUpdatedAsync(id);
    }

    public async Task UpdateCourseAsync(int id, string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds)
    {
        var entity = await SingleCourseAsync(id, nameof(Course.Classes), nameof(Course.Teachers));

        var classes  = await _uow.Classes.GetAsync(c => classIds.Contains(c.Id));
        var teachers = await _uow.Teachers.GetAsync(t => teacherIds.Contains(t.Id));

        entity.Name      = name;
        entity.Year      = year;
        entity.SubjectId = subjectId;
        entity.Classes   = classes.ToList();
        entity.Teachers  = teachers.ToList();

        await _uow.SaveChangesAsync();
        //await _hub.NotifyCourseUpdatedAsync(id);
    }

    public async Task<Course> AddCourseAsync(Course value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        await _uow.Courses.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifyCourseUpdatedAsync(value.Id);

        return value;
    }

    public async Task<Course> AddCourseAsync(string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds)
    {
        var classes  = await _uow.Classes.GetAsync(c => classIds.Contains(c.Id));
        var teachers = await _uow.Teachers.GetAsync(t => teacherIds.Contains(t.Id));

        var value = new Course()
        {
            Name      = name,
            Year      = year,
            SubjectId = subjectId,
            Classes   = classes.ToList(),
            Teachers  = teachers.ToList()
        };

        await _uow.Courses.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifyCourseUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteCourseAsync(int id)
    {
        var entity = await SingleCourseAsync(id);

        _uow.Courses.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifyCourseUpdatedAsync(id);
    }
}