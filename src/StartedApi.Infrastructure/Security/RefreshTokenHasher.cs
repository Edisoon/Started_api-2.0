using System.Security.Cryptography;
using System.Text;
using StartedApi.Application.Security;

namespace StartedApi.Infrastructure.Security;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string refreshToken, string refreshTokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var computedHash = Hash(refreshToken);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(refreshTokenHash));
    }
}
