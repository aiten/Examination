using Microsoft.AspNetCore.SignalR;

using Service;

using WebAPI.Hubs;

namespace WebAPI.Services;

public class HubNotificationService(IHubContext<ExaminationHub, IExaminationHubClient> hubContext) : IHubNotificationService
{
    public Task NotifyExamUpdatedAsync(int examId) =>
        hubContext.Clients.All.ExamUpdated(examId);
}
