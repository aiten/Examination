using System.Threading.Tasks;

namespace Service;

public interface IHubNotificationService
{
    Task NotifyExamUpdatedAsync(int examId);
}
