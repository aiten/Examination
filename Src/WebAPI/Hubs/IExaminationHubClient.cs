namespace WebAPI.Hubs;

public interface IExaminationHubClient
{
    Task ExamUpdated(int examId);
}
