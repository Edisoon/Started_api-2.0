using StartedApi.Domain.Users;

namespace StartedApi.Domain.Auth;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string CreatedByIp { get; set; } = string.Empty;

    public string? RevokedByIp { get; set; }

    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsActive => RevokedAtUtc is null && !IsExpired;
}
