namespace StartedApi.Application.Security;

public interface IRefreshTokenHasher
{
    string Hash(string refreshToken);

    bool Verify(string refreshToken, string refreshTokenHash);
}
