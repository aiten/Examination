namespace Import;

using System.Threading.Tasks;

using Service;

public class DummyHubNotificationService : IHubNotificationService
{
    public async Task NotifyExamUpdatedAsync(int examId) => await Task.CompletedTask;
}