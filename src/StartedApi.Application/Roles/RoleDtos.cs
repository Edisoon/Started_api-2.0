namespace StartedApi.Application.Roles;

public sealed record CreateRoleRequest(
    string Name,
    string? Description = null);

public sealed record UpdateRoleRequest(
    string Name,
    string? Description = null);

public sealed record UpdateRoleStatusRequest(
    bool IsActive,
    string? Reason = null);

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    Guid? UpdatedByUserId,
    DateTime? DeactivatedAtUtc,
    Guid? DeactivatedByUserId);

public sealed record AssignRoleRequest(
    Guid UserId,
    string RoleName);

public sealed record RemoveRoleRequest(
    Guid UserId,
    string RoleName);
