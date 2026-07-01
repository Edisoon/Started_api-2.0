using StartedApi.Application.Auth;
using StartedApi.Application.Common;

namespace StartedApi.Application.Roles;

public interface IRoleService
{
    Task<OperationResult<RoleResponse>> CreateAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<RoleResponse>>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<OperationResult<RoleResponse>> UpdateAsync(
        Guid roleId,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<RoleResponse>> UpdateStatusAsync(
        Guid roleId,
        UpdateRoleStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> AssignAsync(
        AssignRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> RemoveAsync(
        RemoveRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<string>>> GetRolesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
