using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

[Authorize(Policy = Settings.AdminOrUserPolicyName)]
public class ExaminationHub : Hub<IExaminationHubClient>
{
    public async Task JoinExamGroup(int examId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"exam-{examId}");

    public async Task LeaveExamGroup(int examId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"exam-{examId}");
}
