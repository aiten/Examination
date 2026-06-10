namespace WebAPI.Services;

public interface ICurrentUserService
{
    bool IsAdmin { get; }
    Task<int?> GetTeacherIdAsync();
}
