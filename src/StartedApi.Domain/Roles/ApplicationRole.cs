using Microsoft.AspNetCore.Identity;

namespace StartedApi.Domain.Roles;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
