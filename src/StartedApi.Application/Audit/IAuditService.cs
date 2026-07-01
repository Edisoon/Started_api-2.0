namespace StartedApi.Application.Audit;

public interface IAuditService
{
    Task RecordAsync(
        string action,
        Guid? userId = null,
        string? entityName = null,
        string? entityId = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
