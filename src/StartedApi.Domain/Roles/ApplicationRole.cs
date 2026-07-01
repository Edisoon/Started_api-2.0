using Microsoft.AspNetCore.Identity;

namespace StartedApi.Domain.Roles;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTime? DeactivatedAtUtc { get; set; }

    public Guid? DeactivatedByUserId { get; set; }
}
