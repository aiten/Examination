namespace Service;

using Microsoft.Extensions.Logging;

using Persistence;
using Persistence.Model;

using Service.Tools;

using Shared.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Shared;

public interface ICourseService
{
    Task<IList<Course>> GetCoursesAsync(params string[] includeProperties);

    Task<Course> SingleCourseAsync(int id, params string[] includeProperties);

    Task UpdateCourseAsync(int id, string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds, bool canRegister, bool canShowResults, string? pin);

    Task<Course> AddCourseAsync(string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds, bool canRegister, bool canShowResults, string? pin);

    Task DeleteCourseAsync(int id);

    Task<StudentCourse> RegisterStudentAsync(string firstName, string lastName, string pin);
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

    public async Task UpdateCourseAsync(int id, string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds, bool canRegister, bool canShowResults, string? pin)
    {
        var entity = await SingleCourseAsync(id, nameof(Course.Classes), nameof(Course.Teachers));

        var classes  = await _uow.Classes.GetAsync(c => classIds.Contains(c.Id));
        var teachers = await _uow.Teachers.GetAsync(t => teacherIds.Contains(t.Id));

        entity.Name           = name;
        entity.Year           = year;
        entity.SubjectId      = subjectId;
        entity.Classes        = classes.ToList();
        entity.Teachers       = teachers.ToList();
        entity.CanRegister    = canRegister;
        entity.CanShowResults = canShowResults;
        entity.Pin            = pin;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyCourseUpdatedAsync(id);
    }

    public async Task<Course> AddCourseAsync(string name, int year, int subjectId, ICollection<int> classIds, ICollection<int> teacherIds, bool canRegister, bool canShowResults, string? pin)
    {
        var classes  = await _uow.Classes.GetAsync(c => classIds.Contains(c.Id));
        var teachers = await _uow.Teachers.GetAsync(t => teacherIds.Contains(t.Id));

        var value = new Course()
        {
            Name           = name,
            Year           = year,
            SubjectId      = subjectId,
            Classes        = classes.ToList(),
            Teachers       = teachers.ToList(),
            CanRegister    = canRegister,
            CanShowResults = canShowResults,
            Pin            = pin
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

    public async Task<StudentCourse> RegisterStudentAsync(string firstName, string lastName, string pin)
    {
        var course = await _uow.Courses.GetCourseWithPINAsync(pin, includeClasses:true);
        if (course is null)
            throw new IllegalValuesException($"No course found with PIN {pin}");

        if (!course.CanRegister)
            throw new IllegalValuesException($"Course registration with PIN {pin} is not permitted");

        var student = await _uow.Students.GetStudentByNameAsync(lastName, firstName);
        if (student is null)
            throw new IllegalValuesException($"No student found with name '{StudentHelper.FullName(firstName, lastName)}'");

        if (!course.Classes.Any(c => student.Classes.Any(sc => sc.Id == c.Id)))
            throw new IllegalValuesException($"Student '{StudentHelper.FullName(firstName, lastName)} ' is not enrolled in any class of course");

        var registration = await _uow.StudentCourses.GetByStudentAndCourseAsync(student.Id, course.Id);
        
        if (registration is null)
        {
            registration = new StudentCourse()
            {
                StudentId        = student.Id,
                CourseId         = course.Id,
                RegistrationCode = await GenerateUniqueRegistrationCodeAsync(course.Id),
                Student          = student,
                Course           = course
            };
            await _uow.StudentCourses.AddAsync(registration);

        }
        else
        {
            if (registration.RegistrationCode is not null)
            {
                throw new IllegalValuesException($"Student '{StudentHelper.FullName(firstName, lastName)}' is already registered for this course");
            }

            registration.RegistrationCode = await GenerateUniqueRegistrationCodeAsync(course.Id);
        }

        await _uow.SaveChangesAsync();

        return registration;
    }

    private async Task<string> GenerateUniqueRegistrationCodeAsync(int courseId)
    {
        return await ServiceHelper.GenerateUniqueRegistrationCodeAsync(async (code) => await _uow.StudentCourses.AnyAsync(courseId, code));
    }
}