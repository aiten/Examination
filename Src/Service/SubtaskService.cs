namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface ISubtaskService
{
    Task<IList<Subtask>> GetSubtasksForExamAsync(int examId, params string[] includeProperties);

    Task<IList<Subtask>> GetSubtasksAsync(params string[] includeProperties);

    Task<Subtask?> GetSubtaskByIdAsync(int id, params string[] includeProperties);

    Task<Subtask> SingleSubtaskAsync(int id, params string[] includeProperties);

    Task UpdateSubtaskAsync(int id, Subtask value);

    Task<Subtask> AddSubtaskAsync(Subtask value);

    Task DeleteSubtaskAsync(int id);
}

public class SubtaskService : ISubtaskService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<SubtaskService> _logger;
    private readonly IHubNotificationService _hub;

    public SubtaskService(IUnitOfWork uow, ILogger<SubtaskService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Subtask>> GetSubtasksForExamAsync(int examId, params string[] includeProperties)
    {
        return await _uow.Subtasks.GetNoTrackingAsync(s => s.ExamId == examId);
    }

    public async Task<IList<Subtask>> GetSubtasksAsync(params string[] includeProperties)
    {
        return await _uow.Subtasks.GetAsync(null, null, includeProperties);
    }

    public async Task<Subtask?> GetSubtaskByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Subtasks.GetByIdAsync(id, includeProperties);
    }

    public async Task<Subtask> SingleSubtaskAsync(int id, params string[] includeProperties)
    {
        return (await GetSubtaskByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"Subtask {id} not found");
    }

    public async Task UpdateSubtaskAsync(int id, Subtask value)
    {
        var entity = await SingleSubtaskAsync(id);

        if (entity.ExamId != value.ExamId)
        {
            throw new ConflictException($"Must not change ExamId ({entity.ExamId}) for Subtask with ID {id}");
        }

        entity.Description = value.Description;
        entity.SeqNo       = value.SeqNo;
        entity.Points      = value.Points;
        entity.Bonus       = value.Bonus;

        await _uow.SaveChangesAsync();
        //await _hub.NotifySubtaskUpdatedAsync(id);
    }

    public async Task<Subtask> AddSubtaskAsync(Subtask value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        //value.Created  = DateTime.Now;
        //value.Modified = null;

        await _uow.Subtasks.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifySubtaskUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteSubtaskAsync(int id)
    {
        var entity = await SingleSubtaskAsync(id);

        _uow.Subtasks.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifySubtaskUpdatedAsync(id);
    }
}