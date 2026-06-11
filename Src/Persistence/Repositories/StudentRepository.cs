namespace Persistence.Repositories;

using Base.Persistence;
using Base.Persistence.Contracts;

using Microsoft.EntityFrameworkCore;

using Persistence.Model;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task ImportStudentsAsync(string[] studentLines);

    Task<Student?> GetStudentByNameAsync(string lastName, string firstName);
}

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Student?> GetStudentByNameAsync(string lastName, string firstName)
    {
        return await DbSet
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.FirstName == firstName && s.LastName == lastName);
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
                var lastName   = parts[0].Trim();
                var firstName  = parts[1].Trim();
                var classesStr = parts[2].Trim();

                Student? student;
                var      key = (firstName, lastName);

                if (!newStudents.TryGetValue(key, out student))
                {
                    student = await _dbContext.Students
                        .Include(s => s.Classes)
                        .FirstOrDefaultAsync(s => s.FirstName == firstName && s.LastName == lastName);

                    if (student is null)
                    {
                        student = new Student { FirstName = firstName, LastName = lastName };
                        _dbContext.Students.Add(student);
                        newStudents[key] = student;
                    }
                }

                foreach (var entry in classesStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed    = entry.Trim();
                    var parenOpen  = trimmed.IndexOf('(');
                    var parenClose = trimmed.IndexOf(')');

                    if (parenOpen >= 0 && parenClose > parenOpen)
                    {
                        var className = trimmed[..parenOpen].Trim();
                        var yearStr   = trimmed[(parenOpen + 1)..parenClose].Trim();

                        if (int.TryParse(yearStr, out var classYear))
                        {
                            var cls = await _dbContext.Classes
                                .FirstOrDefaultAsync(c => c.Description == className && c.Year == classYear);

                            if (cls is not null && !student.Classes.Any(c => c.Id == cls.Id))
                                student.Classes.Add(cls);
                        }
                    }
                }
            }
        }
    }
}