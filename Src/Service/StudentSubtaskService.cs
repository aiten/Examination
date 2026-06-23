namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface IStudentSubtaskService
{
    Task CheckValid(int id, int examId, int subtaskId);

    Task CheckValidSubtask(int examId, int subtaskId);
    Task CheckValidStudentExam(int examId, int studentExamId);

    Task<IList<StudentSubtask>> GetAllForSubtaskAsync(int examId, int subtaskId);

    Task<IList<StudentSubtask>> GetAllForStudentAsync(int examId, int studentExamId);

    Task<IList<StudentSubtask>> GetStudentSubtasksAsync(params string[] includeProperties);

    Task<StudentSubtask?> GetStudentSubtaskByIdAsync(int id, params string[] includeProperties);

    Task<StudentSubtask> SingleStudentSubtaskAsync(int id, params string[] includeProperties);

    Task UpdateStudentSubtaskAsync(int id, StudentSubtask value);

    Task<StudentSubtask> AddStudentSubtaskAsync(StudentSubtask value);

    Task DeleteStudentSubtaskAsync(int id);
}

public class StudentSubtaskService : IStudentSubtaskService
{
    private readonly IUnitOfWork                    _uow;
    private readonly ILogger<StudentSubtaskService> _logger;
    private readonly IHubNotificationService        _hub;

    public StudentSubtaskService(IUnitOfWork uow, ILogger<StudentSubtaskService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task CheckValid(int id, int examId, int subtaskId)
    {
        var entity = await _uow.StudentSubtasks.GetByIdAsync(id, nameof(StudentSubtask.StudentExam), nameof(StudentSubtask.Subtask));
        if (entity is null)
        {
            throw new NotFoundException($"No StudentSubtask found with ID {id}");
        }

        if (entity.StudentExam.ExamId != examId)
        {
            throw new NotFoundException($"StudentSubtask {id} does not belong to exam {examId}");
        }

        if (entity.SubtaskId != subtaskId)
        {
            throw new NotFoundException($"StudentSubtask {id} does not belong to subtask {subtaskId}");
        }
    }

    public async Task CheckValidSubtask(int examId, int subtaskId)
    {
        var subtask = await _uow.Subtasks.GetByIdAsync(subtaskId);
        if (subtask is null || subtask.ExamId != examId)
        {
            throw new NotFoundException($"No Subtask found with ID {subtaskId} for exam {examId}");
        }
    }

    public async Task CheckValidStudentExam(int examId, int studentExamId)
    {
        var studentExam = await _uow.StudentExams.GetByIdAsync(studentExamId);
        if (studentExam is null || studentExam.ExamId != examId)
        {
            throw new NotFoundException($"No StudentExam found with ID {studentExamId} for exam {examId}");
        }
    }

    public async Task<IList<StudentSubtask>> GetAllForStudentAsync(int examId, int studentExamId)
    {
        return await _uow.StudentSubtasks.GetAllForStudentAsync(examId, studentExamId);
    }

    public async Task<IList<StudentSubtask>> GetAllForSubtaskAsync(int examId, int subtaskId)
    {
        return await _uow.StudentSubtasks.GetAllForSubtaskAsync(examId, subtaskId);
    }

    public async Task<IList<StudentSubtask>> GetStudentSubtasksAsync(params string[] includeProperties)
    {
        return await _uow.StudentSubtasks.GetAsync(null, null, includeProperties);
    }

    public async Task<StudentSubtask?> GetStudentSubtaskByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.StudentSubtasks.GetByIdAsync(id, includeProperties);
    }

    public async Task<StudentSubtask> SingleStudentSubtaskAsync(int id, params string[] includeProperties)
    {
        return (await GetStudentSubtaskByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"StudentSubtask {id} not found");
    }

    public async Task UpdateStudentSubtaskAsync(int id, StudentSubtask value)
    {
        var entity = await SingleStudentSubtaskAsync(id);

        entity.Result         = value.Result;
        entity.Comment        = value.Comment;
        entity.CommentPrivate = value.CommentPrivate;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentSubtaskUpdatedAsync(id);
    }

    public async Task<StudentSubtask> AddStudentSubtaskAsync(StudentSubtask value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        var existing = await _uow.StudentSubtasks.GetNoTrackingAsync(ss => ss.SubtaskId == value.SubtaskId && ss.StudentExamId == value.StudentExamId);

        if (existing.Count > 0)
        {
            throw new ConflictException("A result for the specified student exam and subtask already exists.");
        }

        await _uow.StudentSubtasks.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentSubtaskUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteStudentSubtaskAsync(int id)
    {
        var entity = await SingleStudentSubtaskAsync(id);

        _uow.StudentSubtasks.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifyStudentSubtaskUpdatedAsync(id);
    }
}