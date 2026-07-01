using Microsoft.AspNetCore.Identity;
using StartedApi.Domain.Auth;

namespace StartedApi.Domain.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
