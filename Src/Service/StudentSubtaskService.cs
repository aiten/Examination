namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface IStudentSubtaskService
{
    Task CheckValidSubtask(int     id, int expectedExamId, int expectedSubtaskId);
    Task CheckValidStudentExam(int id, int expectedExamId, int expectedStudentExamId);

    Task<IList<StudentSubtask>> GetAllForSubtaskAsync(int examId, int subtaskId);

    Task<IList<StudentSubtask>> GetAllForStudentAsync(int examId, int studentExamId);


    Task<StudentSubtask?> GetStudentSubtaskByIdAsync(int id, params string[] includeProperties);

    Task<StudentSubtask> SingleStudentSubtaskAsync(int id, params string[] includeProperties);

    Task UpdateStudentSubtaskAsync(int id, int examId, StudentSubtask value);

    Task<StudentSubtask> AddStudentSubtaskAsync(int examId, StudentSubtask value);

    /// <summary>
    /// Imports an evaluation (JSON array of { name, percent, reason }; other properties are ignored) for a student exam.
    /// Subtasks are matched by description; unmatched entries are reported in the result.
    /// </summary>
    Task<StudentSubtaskImportResult> ImportStudentSubtasksAsync(int examId, int studentExamId, string content);
}

public record StudentSubtaskImportResult(int Imported, IList<string> NotFound);

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

    public async Task CheckValidSubtask(int id, int expectedExamId, int expectedSubtaskId)
    {
        var entity = await _uow.StudentSubtasks.GetByIdAsync(id, nameof(StudentSubtask.StudentExam), nameof(StudentSubtask.Subtask));
        if (entity is null)
        {
            throw new NotFoundException($"No StudentSubtask found with ID {id}");
        }

        if (entity.StudentExam.ExamId != expectedExamId)
        {
            throw new ConflictException($"StudentSubtask {id} does not belong to exam {expectedExamId}");
        }

        if (entity.SubtaskId != expectedSubtaskId)
        {
            throw new ConflictException($"StudentSubtask {id} does not belong to subtask {expectedSubtaskId}");
        }
    }

    public async Task CheckValidStudentExam(int id, int expectedExamId, int expectedStudentExamId)
    {
        var entity = await _uow.StudentSubtasks.GetByIdAsync(id, nameof(StudentSubtask.StudentExam), nameof(StudentSubtask.Subtask));
        if (entity is null)
        {
            throw new NotFoundException($"No StudentSubtask found with ID {id}");
        }

        if (entity.Subtask.ExamId != expectedExamId)
        {
            throw new ConflictException($"StudentSubtask {id} does not belong to exam {expectedExamId}");
        }

        if (entity.StudentExamId != expectedStudentExamId)
        {
            throw new ConflictException($"StudentSubtask {id} does not belong to studentexam {expectedStudentExamId}");
        }
    }

    public async Task CheckSubtaskBelongsToExam(int expectedExamId, int subtaskId)
    {
        var subtask = await _uow.Subtasks.GetByIdAsync(subtaskId);
        if (subtask is null || subtask.ExamId != expectedExamId)
        {
            throw new ConflictException($"No Subtask found with ID {subtaskId} for exam {expectedExamId}");
        }
    }

    public async Task CheckStudentExamBelongsToExam(int expectedExamId, int studentExamId)
    {
        var studentExam = await _uow.StudentExams.GetByIdAsync(studentExamId);
        if (studentExam is null || studentExam.ExamId != expectedExamId)
        {
            throw new ConflictException($"No StudentExam found with ID {studentExamId} for exam {expectedExamId}");
        }
    }

    public async Task<IList<StudentSubtask>> GetAllForStudentAsync(int examId, int studentExamId)
    {
        await CheckStudentExamBelongsToExam(examId, studentExamId);
        return await _uow.StudentSubtasks.GetAllForStudentAsync(examId, studentExamId);
    }

    public async Task<IList<StudentSubtask>> GetAllForSubtaskAsync(int examId, int subtaskId)
    {
        await CheckSubtaskBelongsToExam(examId, subtaskId);
        return await _uow.StudentSubtasks.GetAllForSubtaskAsync(examId, subtaskId);
    }

    public async Task<StudentSubtask?> GetStudentSubtaskByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.StudentSubtasks.GetByIdAsync(id, includeProperties);
    }

    public async Task<StudentSubtask> SingleStudentSubtaskAsync(int id, params string[] includeProperties)
    {
        return (await GetStudentSubtaskByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"StudentSubtask {id} not found");
    }

    public async Task UpdateStudentSubtaskAsync(int id, int examId, StudentSubtask value)
    {
        await CheckValidSubtask(id, examId, value.SubtaskId);

        var entity = await SingleStudentSubtaskAsync(id);

        if (entity.StudentExamId != value.StudentExamId)
        {
            throw new ConflictException($"StudentSubtask {id} does not belong to studentexam {value.StudentExamId}");
        }

        entity.Result         = value.Result;
        entity.Comment        = value.Comment;
        entity.CommentPrivate = value.CommentPrivate;
        entity.Date           = value.Date;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyStudentSubtaskUpdatedAsync(id);
    }

    public async Task<StudentSubtask> AddStudentSubtaskAsync(int examId, StudentSubtask value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        await CheckSubtaskBelongsToExam(examId, value.SubtaskId);
        await CheckStudentExamBelongsToExam(examId, value.StudentExamId);

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

    private const int MaxCommentLength = 2000;

    private static readonly JsonSerializerOptions ImportJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling              = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling         = JsonCommentHandling.Skip,
        AllowTrailingCommas         = true
    };

    private sealed record ImportItem(string? Name, decimal? Percent, string? Reason);

    public async Task<StudentSubtaskImportResult> ImportStudentSubtasksAsync(int examId, int studentExamId, string content)
    {
        var items = ParseImport(content);

        await CheckStudentExamBelongsToExam(examId, studentExamId);

        var subtasks = await _uow.Subtasks.GetNoTrackingAsync(st => st.ExamId == examId);
        var existing = await _uow.StudentSubtasks.GetAsync(ss => ss.StudentExamId == studentExamId);

        var imported = 0;
        var notFound = new List<string>();

        foreach (var item in items)
        {
            var name    = item.Name!.Trim();
            var subtask = subtasks.FirstOrDefault(st => string.Equals(st.Description.Trim(), name, StringComparison.OrdinalIgnoreCase));
            if (subtask is null)
            {
                notFound.Add(name);
                continue;
            }

            var result  = item.Percent.HasValue ? Math.Round(item.Percent.Value) / 100M : (decimal?)null;
            var comment = string.IsNullOrWhiteSpace(item.Reason) ? null : item.Reason.Trim();
            if (comment is { Length: > MaxCommentLength })
            {
                comment = comment[..MaxCommentLength];
            }

            var entity = existing.FirstOrDefault(ss => ss.SubtaskId == subtask.Id);
            if (entity is null)
            {
                entity = new StudentSubtask { StudentExamId = studentExamId, SubtaskId = subtask.Id };
                await _uow.StudentSubtasks.AddAsync(entity);
                existing.Add(entity);
            }

            entity.Result  = result;
            entity.Comment = comment;
            imported++;
        }

        await _uow.SaveChangesAsync();

        return new StudentSubtaskImportResult(imported, notFound);
    }

    private static IList<ImportItem> ParseImport(string content)
    {
        var json = content.Trim();

        // tolerate a markdown code fence around the JSON as copied from a chat
        if (json.StartsWith("```"))
        {
            var start = json.IndexOf('\n');
            var end   = json.LastIndexOf("```", StringComparison.Ordinal);
            if (start >= 0 && end > start)
            {
                json = json[(start + 1)..end].Trim();
            }
        }

        if (json.Length == 0)
        {
            throw new IllegalValuesException("Import content is empty");
        }

        // tolerate a list of objects without surrounding brackets and with a trailing comma
        if (json.StartsWith('{'))
        {
            json = "[" + json.TrimEnd(',', ' ', '\r', '\n', '\t') + "]";
        }

        List<ImportItem>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<ImportItem>>(json, ImportJsonOptions);
        }
        catch (JsonException ex)
        {
            throw new IllegalValuesException($"Import content is not valid JSON: {ex.Message}");
        }

        if (items is null || items.Count == 0)
        {
            throw new IllegalValuesException("Import content contains no entries");
        }

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item is null || string.IsNullOrWhiteSpace(item.Name))
            {
                throw new IllegalValuesException($"Entry {i + 1} has no 'name'");
            }

            if (item.Percent is < 0 or > 100)
            {
                throw new IllegalValuesException($"Entry '{item.Name}': 'percent' must be between 0 and 100");
            }
        }

        return items;
    }
}