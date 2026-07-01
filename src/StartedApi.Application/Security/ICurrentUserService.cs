namespace StartedApi.Application.Security;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    bool IsAuthenticated { get; }
}
