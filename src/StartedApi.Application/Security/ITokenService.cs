namespace StartedApi.Application.Security;

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

public sealed record RefreshTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

public interface ITokenService
{
    Task<AccessTokenResult> GenerateAccessTokenAsync(
        Guid userId,
        string email,
        string userName,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default);

    RefreshTokenResult GenerateRefreshToken();
}
