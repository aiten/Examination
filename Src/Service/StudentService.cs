namespace Service;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Persistence;
using Persistence.Model;

using Shared.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface IStudentService
{
    Task<IList<Student>> GetStudentsAsync(params string[] includeProperties);

    Task<Student?> GetStudentByIdAsync(int id, params string[] includeProperties);

    Task<Student> SingleStudentAsync(int id, params string[] includeProperties);

    Task UpdateStudentAsync(int id, string firstName, string lastname, ICollection<int> classIds);

    Task<Student> AddStudentAsync(string firstName, string lastname, ICollection<int> classIds);

    Task DeleteStudentAsync(int       id);
    Task ImportStudentsAsync(string[] studentLines);
}

public class StudentService : IStudentService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<StudentService>   _logger;
    private readonly IHubNotificationService _hub;

    public StudentService(IUnitOfWork uow, ILogger<StudentService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Student>> GetStudentsAsync(params string[] includeProperties)
    {
        return await _uow.Students.GetAsync(null, null, includeProperties);
    }

    public async Task<Student?> GetStudentByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Students.GetByIdAsync(id, includeProperties);
    }

    public async Task<Student> SingleStudentAsync(int id, params string[] includeProperties)
    {
        return (await GetStudentByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"Student {id} not found");
    }

    public async Task UpdateStudentAsync(int id, string firstName, string lastname, ICollection<int> classIds)
    {
        var classes = await _uow.Classes.GetAsync(c => classIds.Contains(c.Id));

        var entity  = await SingleStudentAsync(id, nameof(Student.Classes));

        entity.FirstName = firstName;
        entity.LastName = lastname;

        entity.Classes.Clear();
        foreach (var cls in classes)
        {
            entity.Classes.Add(cls);
        }

        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentUpdatedAsync(id);
    }

    public async Task<Student> AddStudentAsync(string firstName, string lastname, ICollection<int> classIds)
    {
        var classes = await _uow.Classes.GetAsync(c => classIds.Contains(c.Id));

        var value = new Student
        {
            FirstName = firstName,
            LastName = lastname,
            Classes = classes
        };

        await _uow.Students.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteStudentAsync(int id)
    {
        var entity = await SingleStudentAsync(id);

        _uow.Students.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifyStudentUpdatedAsync(id);
    }

    public async Task ImportStudentsAsync(string[] studentLines)
    {
        //format of string: "LastName; FirstName; Class1 (Year), Class2 (Year), ..."

        var newStudents = new Dictionary<(string, string), Student>();

        foreach (var line in studentLines)
        {
            var parts = line.Split(';');
            if (parts.Length >= 3)
            {
                var lastName = parts[0].Trim();
                var firstName = parts[1].Trim();
                var classesStr = parts[2].Trim();

                Student? student;
                var key = (firstName, lastName);

                if (!newStudents.TryGetValue(key, out student))
                {
                    student = await _uow.Students.GetStudentByNameAsync(lastName, firstName);

                    if (student is null)
                    {
                        student = new Student { FirstName = firstName, LastName = lastName };
                        await _uow.Students.AddAsync(student);
                        newStudents[key] = student;
                    }
                }

                foreach (var entry in classesStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = entry.Trim();
                    var parenOpen = trimmed.IndexOf('(');
                    var parenClose = trimmed.IndexOf(')');

                    if (parenOpen >= 0 && parenClose > parenOpen)
                    {
                        var className = trimmed[..parenOpen].Trim();
                        var yearStr = trimmed[(parenOpen + 1)..parenClose].Trim();

                        if (int.TryParse(yearStr, out var classYear))
                        {
                            var cls = await _uow.Classes.GetClassByNameAndYearAsync(className,classYear);

                            if (cls is not null && !student.Classes.Any(c => c.Id == cls.Id))
                                student.Classes.Add(cls);
                        }
                    }
                }
            }
        }
    }

}