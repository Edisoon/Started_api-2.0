using StartedApi.Domain.Users;

namespace StartedApi.Domain.Audit;

public class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string? Details { get; set; }
}
