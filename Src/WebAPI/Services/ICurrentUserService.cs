namespace Core.Contracts;

public interface ICurrentUserService
{
    bool IsAdmin { get; }
    Task<int?> GetTeacherIdAsync();
}
