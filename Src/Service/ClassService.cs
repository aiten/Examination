namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

public interface IClassService
{
    Task<IList<Class>> GetClassesAsync(params string[] includeProperties);

    Task<Class?> GetClassByIdAsync(int id, params string[] includeProperties);

    Task<Class> SingleClassAsync(int id, params string[] includeProperties);

    Task UpdateClassAsync(int id, Class value);

    Task<Class> AddClassAsync(Class value);

    Task DeleteClassAsync(int id);
}

public class ClassService : IClassService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<ClassService>   _logger;
    private readonly IHubNotificationService _hub;

    public ClassService(IUnitOfWork uow, ILogger<ClassService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<IList<Class>> GetClassesAsync(params string[] includeProperties)
    {
        return await _uow.Classes.GetAsync(null, null, includeProperties);
    }

    public async Task<Class?> GetClassByIdAsync(int id, params string[] includeProperties)
    {
        return await _uow.Classes.GetByIdAsync(id, includeProperties);
    }

    public async Task<Class> SingleClassAsync(int id, params string[] includeProperties)
    {
        return (await GetClassByIdAsync(id, includeProperties)) ?? throw new NotFoundException($"Class {id} not found");
    }

    public async Task UpdateClassAsync(int id, Class value)
    {
        var entity = await SingleClassAsync(id);

        entity.Description = value.Description;
        entity.Year        = value.Year;
        entity.TeacherId   = value.TeacherId;

        await _uow.SaveChangesAsync();
        //await _hub.NotifyClassUpdatedAsync(id);
    }

    public async Task<Class> AddClassAsync(Class value)
    {
        if (value.Id != 0)
        {
            throw new IllegalValuesException("Id must be 0 for new entities");
        }

        //value.Created  = DateTime.Now;
        //value.Modified = null;

        await _uow.Classes.AddAsync(value);
        await _uow.SaveChangesAsync();
        //await _hub.NotifyClassUpdatedAsync(value.Id);

        return value;
    }

    public async Task DeleteClassAsync(int id)
    {
        var entity = await SingleClassAsync(id);

        _uow.Classes.Remove(entity);
        await _uow.SaveChangesAsync();
        // await _hub.NotifyClassUpdatedAsync(id);
    }
}