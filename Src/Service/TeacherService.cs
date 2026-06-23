namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface ITeacherService
{
    Task<IList<Teacher>> GetTeachersAsync(params string[] includeProperties);

    Task<Teacher?> GetTeacherByIdAsync(int id, params string[] includeProperties);

    Task<Teacher> SingleTeacherAsync(int id, params string[] includeProperties);

    Task UpdateTeacherAsync(int id, Teacher value);

    Task<Teacher> AddTeacherAsync(Teacher value);

    Task DeleteTeacherAsync(int id);
}

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<TeacherService>   _logger;
    private readonly IHubNotificationService _hub;

    public TeacherService(IUnitOfWork uow, ILogger<TeacherService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Teacher>> GetTeachersAsync(params string[] includeProperties)
    {
        return await _uow.Teachers.GetAsync(null, null, includeProperties);
    }

    public async Task<Teacher?> GetTeacherByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Teachers.GetByIdAsync(id, includeProperties);
    }

    public async Task<Teacher> SingleTeacherAsync(int id, params string[] includeProperties)
    {
        return (await GetTeacherByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"Teacher {id} not found");
    }

    public async Task UpdateTeacherAsync(int id, Teacher value)
    {
        var entity = await SingleTeacherAsync(id);

        entity.FirstName    = value.FirstName;
        entity.LastName     = value.LastName;
        entity.NickName     = value.NickName;
        entity.Abbreviation = value.Abbreviation;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyTeacherUpdatedAsync(id);
    }

    public async Task<Teacher> AddTeacherAsync(Teacher value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        //value.Created  = DateTime.Now;
        //value.Modified = null;

        await _uow.Teachers.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifyTeacherUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteTeacherAsync(int id)
    {
        var entity = await SingleTeacherAsync(id);

        _uow.Teachers.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifyTeacherUpdatedAsync(id);
    }
}