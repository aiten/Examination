using Base.Tools.CsvImport;

using Import.ImportData;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Persistence;

using System;
using System.Collections.Generic;
using System.Linq;

using Persistence.Model;
using Base.Tools;

var builder = Host.CreateApplicationBuilder(args);


var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services
    .AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString))
    .AddScoped<IUnitOfWork, UnitOfWork>()
    .AddAssemblyIncludingInternals(name => name.EndsWith("Repository"), ServiceLifetime.Transient, typeof(ApplicationDbContext).Assembly)
    ;

var host = builder.Build();

Console.WriteLine("Migrate Database");

using (var scope = host.Services.CreateScope())
{
    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//    await uow.DeleteDatabaseAsync();
//    await uow.CreateDatabaseAsync();
    await uow.MigrateDatabaseAsync();
}

Console.WriteLine("Import Data");

bool doImport = false;

if (doImport)
{
    using (var scope = host.Services.CreateScope())
    {
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var classesCsv = await new CsvImport<ClassesCsv>().ReadAsync("ImportData/Classes2025.csv");

        var sprechstunden = await new CsvImport<SprechstundeCsv>().ReadAsync("ImportData/Sprechstunden2025.csv");

        var teachersCsv = sprechstunden
            .Where(s => s.Lehrkraft != "Abendschule Chemie")
            .Select(s =>
            {
                var name = s.Lehrkraft.Split(' ');
                return new Teacher()
                {
                    FirstName = name[1],
                    LastName  = name[0],
                };
            })
            .ToList();


        var examCsvs = await new CsvImport<ExamCsv>().ReadAsync("ImportData/Exam.csv");

        var teachersExam = examCsvs
            .Select(c => c.Teacher)
            .Distinct()
            .Select(t =>
            {
                var names = t.Split(' ');
                return new Teacher()
                {
                    LastName  = names[0],
                    FirstName = names.Length > 1 ? names[1] : string.Empty
                };
            })
            .ToList();


        var teacherBoth = teachersExam.Join(teachersCsv,
            t => t.LastName,
            t => t.LastName,
            (q, csv) => (q, csv));

        foreach (var x in teacherBoth)
        {
            if (!string.IsNullOrEmpty(x.csv.FirstName))
            {
                x.q.FirstName = x.csv.FirstName;
            }

            teachersCsv.Remove(x.csv);
        }

        // Merge abbreviations from Classes.csv into teachers; collect teachers only in Classes.csv
        var allTeachers         = teachersExam.Concat(teachersCsv).ToList();
        var classesOnlyTeachers = new List<Teacher>();

        Teacher? GetTeacherWithName(string name)
        {
            var names = name.Split(' ');
            return GetTeacher(names[0], names.Length > 1 ? names[1] : string.Empty);
        }

        Teacher? GetTeacher(string lastName, string? firstName)
        {
            var teacher = allTeachers.FirstOrDefault(t =>
                t.LastName == lastName &&
                t.FirstName == firstName);

            if (teacher == null)
            {
                teacher = allTeachers.FirstOrDefault(t =>
                    t.LastName == lastName);
            }

            return teacher;
        }

        foreach (var classRow in classesCsv)
        {
            var teacher = GetTeacher(classRow.LastName, classRow.FirstName);

            if (teacher == null)
            {
                teacher = new Teacher()
                {
                    FirstName    = classRow.FirstName,
                    LastName     = classRow.LastName,
                    Abbreviation = classRow.Abbreviation
                };
                allTeachers.Add(teacher);
                classesOnlyTeachers.Add(teacher);
            }
            else
            {
                teacher.Abbreviation = classRow.Abbreviation;
            }
        }

        // Add school classes with reference to main teacher
        var classes = classesCsv
            .Select(c =>
            {
                var teacher = allTeachers.FirstOrDefault(t => t.LastName == c.LastName);
                return new Class()
                {
                    Description = c.ClassName,
                    Year        = 2025,
                    Teacher     = teacher
                };
            })
            .ToList();

        var teachersSubjects = examCsvs
            .GroupBy(ex => (ex.Class, ex.Year, ex.Subject))
            .Select(c => new Subject()
                {
                    Name = $"{c.Key.Subject}/{c.Key.Class}",
                    Courses = new Course[]
                    {
                        new Course()
                        {
                            Name     = $"{c.Key.Subject}/{c.Key.Class}/{c.Key.Year}",
                            Year     = c.Key.Year,
                            Teachers = new[] { GetTeacherWithName(c.First().Teacher)! },
                            Classes  = classes.Where(cl => cl.Description == c.Key.Class && cl.Year == c.Key.Year).ToList()
                        }
                    }
                }
            ).ToList();

        var teachexam = examCsvs
            .Select(c => new Exam()
            {
                Created     = DateTime.Now,
                Date        = c.Date,
                From        = c.From,
                To          = c.To,
                Pin         = c.PIN,
                Description = c.Description,
                Subtasks = (c.SubTasks ?? "").Split(',').Select((l, idx) => new Subtask()
                {
                    SeqNo       = idx + 1,
                    Description = l.Trim(),
                    Points      = 1
                }).ToList(),
                Teacher = GetTeacherWithName(c.Teacher)!,
                Course  = teachersSubjects.Single(s => s.Name == $"{c.Subject}/{c.Class}").Courses.First(cc => cc.Name == $"{c.Subject}/{c.Class}/{c.Year}")
            })
            .ToList();

        await uow.Subjects.AddRangeAsync(teachersSubjects);

        await uow.Teachers.AddRangeAsync(teachersExam);
        await uow.Teachers.AddRangeAsync(teachersCsv);
        await uow.Teachers.AddRangeAsync(classesOnlyTeachers);

        await uow.Classes.AddRangeAsync(classes);

        await uow.Exams.AddRangeAsync(teachexam);

        await uow.SaveChangesAsync();
    }

    Console.WriteLine("done");
}