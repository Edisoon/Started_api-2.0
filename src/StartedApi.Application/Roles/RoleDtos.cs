namespace StartedApi.Application.Roles;

public sealed record CreateRoleRequest(
    string Name,
    string? Description = null);

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string? Description);

public sealed record AssignRoleRequest(
    Guid UserId,
    string RoleName);

public sealed record RemoveRoleRequest(
    Guid UserId,
    string RoleName);
