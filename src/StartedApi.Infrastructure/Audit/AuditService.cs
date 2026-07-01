using StartedApi.Application.Audit;
using StartedApi.Domain.Audit;
using StartedApi.Infrastructure.Persistence;

namespace StartedApi.Infrastructure.Audit;

public sealed class AuditService : IAuditService
{
    private readonly ApplicationDbContext _dbContext;

    public AuditService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(
        string action,
        Guid? userId = null,
        string? entityName = null,
        string? entityId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName ?? string.Empty,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OccurredAtUtc = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
