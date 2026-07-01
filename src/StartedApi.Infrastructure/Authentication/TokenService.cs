using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StartedApi.Application.Security;

namespace StartedApi.Infrastructure.Authentication;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public Task<AccessTokenResult> GenerateAccessTokenAsync(
        Guid userId,
        string email,
        string userName,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var signingKey = CreateSigningKey();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, userName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(new AccessTokenResult(accessToken, expiresAtUtc));
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var expiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        return new RefreshTokenResult(token, expiresAtUtc);
    }

    private SymmetricSecurityKey CreateSigningKey()
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Secret) || _jwtOptions.Secret.Length < 32)
        {
            throw new InvalidOperationException("JWT secret must contain at least 32 characters.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
    }
}
