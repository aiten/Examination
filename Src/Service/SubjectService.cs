namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface ISubjectService
{
    Task<IList<Subject>> GetSubjectsAsync(params string[] includeProperties);

    Task<Subject?> GetSubjectByIdAsync(int id, params string[] includeProperties);

    Task<Subject> SingleSubjectAsync(int id, params string[] includeProperties);

    Task UpdateSubjectAsync(int id, Subject value);

    Task<Subject> AddSubjectAsync(Subject value);

    Task DeleteSubjectAsync(int id);
}

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<SubjectService>   _logger;
    private readonly IHubNotificationService _hub;

    public SubjectService(IUnitOfWork uow, ILogger<SubjectService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Subject>> GetSubjectsAsync(params string[] includeProperties)
    {
        return await _uow.Subjects.GetAsync(null, null, includeProperties);
    }

    public async Task<Subject?> GetSubjectByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Subjects.GetByIdAsync(id, includeProperties);
    }

    public async Task<Subject> SingleSubjectAsync(int id, params string[] includeProperties)
    {
        return (await GetSubjectByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"Subject {id} not found");
    }

    public async Task UpdateSubjectAsync(int id, Subject value)
    {
        var entity = await SingleSubjectAsync(id);

        entity.Name    = value.Name;
        entity.Comment = value.Comment;

        await _uow.SaveChangesAsync();
        //await _hub.NotifySubjectUpdatedAsync(id);
    }

    public async Task<Subject> AddSubjectAsync(Subject value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        //value.Created  = DateTime.Now;
        //value.Modified = null;

        await _uow.Subjects.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifySubjectUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteSubjectAsync(int id)
    {
        var entity = await SingleSubjectAsync(id);

        _uow.Subjects.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifySubjectUpdatedAsync(id);
    }
}