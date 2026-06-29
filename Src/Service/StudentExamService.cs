namespace Service;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Persistence;
using Persistence.Model;
using Persistence.QueryResult;

using Service.Tools;

using Shared;
using Shared.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public interface IStudentExamService
{
    Task<StudentExamResult> GetStudentExamResultAsync(string firstName, string lastName, string pin, string registrationCode);

    Task<StudentCourseResult> GetStudentCourseResultAsync(string firstName, string lastName, string pin, string registrationCode);

    Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId);

    Task<IList<StudentExamSummary>> GetStudentExamSummaryAsync(int examId);

    Task<StudentExam> SingleStudentExamAsync(int id, params string[] includeProperties);

    Task UpdateStudentExamAsync(int id, StudentExam value);

    Task DeleteStudentExamAsync(int id);
}

public class StudentExamService : IStudentExamService
{
    private readonly IUnitOfWork                 _uow;
    private readonly ILogger<StudentExamService> _logger;
    private readonly IHubNotificationService     _hub;

    public StudentExamService(IUnitOfWork uow, ILogger<StudentExamService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<StudentExamResult> GetStudentExamResultAsync(string firstName, string lastName, string pin, string registrationCode)
    {
        var exam = await _uow.Exams.GetExamWithPINAsync(pin) ?? throw new NotFoundException($"No exam found with pin: {pin}.");

        if (!exam.CanShowResults)
            throw new NotFoundException("Results are not yet available for this exam.");

        var studentExam = await _uow.StudentExams.GetStudentExamAsync(exam.Id, firstName, lastName, registrationCode) ?? throw new NotFoundException($"No exam found with student");

        var result = await GetStudentResultAsync(studentExam, exam);

        return new StudentExamResult(
            null,
            exam.Description,
            exam.ExamType,
            exam.Date,
            StudentHelper.FullName(studentExam.Student.FirstName, studentExam.Student.LastName),
            result.resultSubtasks,
            result.totalPoints,
            result.percent,
            result.grade);
    }

    private async Task<(IList<StudentExamResultSubtask> resultSubtasks, decimal? totalPoints, decimal? percent, int? grade)> GetStudentResultAsync(StudentExam studentExam, Exam exam)
    {
        var subtasks           = await _uow.Subtasks.GetForExamAsync(exam.Id);
        var studentSubtasksDict = studentExam.StudentSubtasks.ToDictionary(sst => sst.SubtaskId);

        var resultSubtasks = subtasks
            .OrderBy(s => s.SeqNo)
            .Select(s =>
            {
                studentSubtasksDict.TryGetValue(s.Id, out var ss);
                return new StudentExamResultSubtask(s.SeqNo, s.Description, s.Points, ss?.Result, ss?.Comment, s.Bonus,ss?.Date);
            })
            .ToList();

        var totalMaxPoints = subtasks.Sum(s => s.Bonus ? 0 : s.Points);
        var countRatable   = subtasks.Count(s => !s.Bonus);

        if (exam.ExamType == ExamType.Participation)
        {
            // only with result are counting
            resultSubtasks = resultSubtasks.Where(r => r.Result.HasValue).ToList();

            totalMaxPoints = resultSubtasks.Where(r => r.Bonus == false).Sum(r => r.Points);
            countRatable   = resultSubtasks.Count(r => r.Bonus == false);
        }

        var countRated = studentExam.StudentSubtasks.Count(ss => ss.Result.HasValue && !ss.Subtask.Bonus);
        var allRated   = countRated == countRatable;

        if (!allRated)
            throw new NotFoundException("Results are not yet available for this exam.");

        var totalPoints = (decimal?)studentExam.StudentSubtasks.Sum(ss => (ss.Result ?? 0m) * ss.Subtask.Points);
        var percent     = totalMaxPoints > 0 ? Math.Round(totalPoints!.Value / totalMaxPoints * 100m, 2) : (decimal?)null;
        var grade       = totalMaxPoints > 0 ? (int?)ExamHelper.CalculateGrade(totalPoints!.Value / totalMaxPoints) : null;

        return (resultSubtasks, totalPoints, percent, grade);
    }


    public async Task<StudentCourseResult> GetStudentCourseResultAsync(string firstName, string lastName, string pin, string registrationCode)
    {
        var course = await _uow.Courses.GetCourseWithPINAsync(pin, includeExams: true) ?? throw new NotFoundException($"No course found with pin: {pin}.");

        if (!course.CanShowResults)
            throw new NotFoundException("Results are not available for this course.");

        var studentCourse = await _uow.StudentCourses.GetStudentCourseAsync(course.Id, firstName, lastName, registrationCode) ?? throw new NotFoundException($"No course found for student");

        var resultList = new List<StudentExamResult>();

        foreach (var exam in course.Exams)
        {
            StudentExamResult examResult;

            StudentExamResult ErrorResult(string status) => new StudentExamResult(status, exam.Description, exam.ExamType, exam.Date, StudentHelper.FullName(studentCourse.Student.FirstName, studentCourse.Student.LastName), [], null, null, null);

            if (exam.CanShowResults)
            {
                var studentExam = await _uow.StudentExams.GetStudentExamAsync(exam.Id, studentCourse.StudentId);

                if (studentExam is null)
                {
                    examResult = ErrorResult("Not registered for this exam.");
                }
                else
                {
                    try
                    {
                        var result = await GetStudentResultAsync(studentExam, exam);
                        examResult = new StudentExamResult(
                            null,
                            exam.Description,
                            exam.ExamType,
                            exam.Date,
                            StudentHelper.FullName(studentExam.Student.FirstName, studentExam.Student.LastName),
                            result.resultSubtasks,
                            result.totalPoints,
                            result.percent,
                            result.grade);
                    }
                    catch (NotFoundException e)
                    {
                        examResult = ErrorResult(e.Message);
                    }
                }
            }
            else
            {
                examResult = ErrorResult("Results are not yet available for this exam.");
            }

            resultList.Add(examResult);
        }

        return new StudentCourseResult(course.Name, studentCourse.Student.FullName, resultList);
    }

    public async Task<IList<StudentExamOverview>> GetStudentExamOverviewsAsync(int examId)
    {
        var exam = await _uow.Exams.GetByIdAsync(examId,
                       nameof(Exam.StudentExams),
                       $"{nameof(Exam.StudentExams)}.{nameof(StudentExam.StudentSubtasks)}",
                       $"{nameof(Exam.StudentExams)}.{nameof(StudentExam.Student)}",
                       nameof(Exam.Subtasks))
                   ?? throw new NotFoundException($"No exam found with id: {examId}.");

        var subtasks = exam.Subtasks;

        bool isParticipation = exam.ExamType == ExamType.Participation;

        var totalMaxPoints = subtasks.Sum(s => s.Bonus ? 0 : s.Points);
        var countRatable   = subtasks.Count(s => s.Bonus == false);

        var result = exam.StudentExams
            .Select(se => new
            {
                se.Id,
                se.StudentId,
                se.Student.FirstName,
                se.Student.LastName,
                se.LoginName,
                se.RegistrationCode,
                Points       = (decimal)se.StudentSubtasks.Sum(ss => (double)(ss.Result ?? 0m) * (double)ss.Subtask.Points),
                TotalPoints  = isParticipation ?  se.StudentSubtasks.Sum(sst => sst.Result.HasValue && sst.Subtask.Bonus==false ? sst.Subtask.Points : 0) : totalMaxPoints,
                CountRated   = se.StudentSubtasks.Count(ss => ss.Result.HasValue && ss.Subtask.Bonus == false),
                CountRatable = isParticipation ? se.StudentSubtasks.Count(ss => ss.Result.HasValue && ss.Subtask.Bonus == false) : countRatable
            })
            .ToList();

        return result.Select(r => new StudentExamOverview(
            r.Id,
            r.StudentId,
            r.FirstName,
            r.LastName,
            r.LoginName,
            r.RegistrationCode,
            r.CountRated,
            r.CountRated == r.CountRatable ? r.Points : null,
            r.TotalPoints != 0 && r.CountRated == r.CountRatable ? Math.Round(r.Points / r.TotalPoints * 100m, 2) : null,
            r.TotalPoints != 0 && r.CountRated == r.CountRatable ? ExamHelper.CalculateGrade(r.Points / r.TotalPoints) : null
        )).ToList();
    }

    public async Task<IList<StudentExamSummary>> GetStudentExamSummaryAsync(int examId)
    {
        var studenResults = await GetStudentExamOverviewsAsync(examId);
        return studenResults
            .GroupBy(r => r.Grade)
            .Select(g => new StudentExamSummary(g.Key, g.Count()))
            .ToList();
    }

    public async Task<StudentExam?> GetStudentExamByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.StudentExams.GetByIdAsync(id, includeProperties);
    }

    public async Task<StudentExam> SingleStudentExamAsync(int id, params string[] includeProperties)
    {
        return (await GetStudentExamByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"StudentExam {id} not found");
    }

    public async Task UpdateStudentExamAsync(int id, StudentExam value)
    {
        var entity = await SingleStudentExamAsync(id);

        if (entity.ExamId != value.ExamId)
        {
            throw new ConflictException($"Must not change ExamId ({entity.ExamId}) for StudentExam with ID {id}");
        }

        entity.LoginName        = value.LoginName;
        entity.RegistrationCode = value.RegistrationCode;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentExamUpdatedAsync(id);
    }

    public async Task DeleteStudentExamAsync(int id)
    {
        var entity = await SingleStudentExamAsync(id, nameof(StudentExam.StudentSubtasks));

        if (entity.StudentSubtasks.Count(s => s.Result is not null) > 0)
        {
            throw new BusinessRuleException("StudentExam has results and cannot be deleted.");
        }

        await _uow.StudentExams.DeleteAsync(entity);

        await _uow.SaveChangesAsync();
        // await _hub.NotifyStudentExamUpdatedAsync(id);
    }
}