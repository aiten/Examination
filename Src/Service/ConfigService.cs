namespace Service;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Persistence;

using Shared.Exceptions;

using Persistence.Model;

using Service.Tools;

public interface IConfigService
{
    Task<int> GetCurrentSchoolYear();
}

public class ConfigService : IConfigService
{
    private readonly IUnitOfWork             _uow;
    private readonly ILogger<TeacherService>   _logger;
    private readonly IHubNotificationService _hub;

    public ConfigService(IUnitOfWork uow, ILogger<TeacherService> logger, IHubNotificationService hub)
    {
        _uow    = uow;
        _logger = logger;
        _hub    = hub;
    }

    public async Task<int> GetCurrentSchoolYear()
    {
        return await Task.FromResult(ServiceHelper.GetCurrentSchoolYear());
    }
}