using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartedApi.Application.Audit;
using StartedApi.Application.Auth;
using StartedApi.Application.Common;
using StartedApi.Application.Roles;
using StartedApi.Application.Security;
using StartedApi.Domain.Audit;
using StartedApi.Domain.Roles;
using StartedApi.Domain.Users;

namespace StartedApi.Infrastructure.Roles;

public sealed class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<OperationResult<RoleResponse>> CreateAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            return OperationResult<RoleResponse>.Failure("Role already exists.");
        }

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return OperationResult<RoleResponse>.Failure("Role creation failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.RoleCreated, _currentUserService.UserId, nameof(ApplicationRole), role.Id.ToString(), $"Role {request.Name} created.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        return OperationResult<RoleResponse>.Success(ToResponse(role));
    }

    public async Task<OperationResult<IReadOnlyList<RoleResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => ToResponse(role))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<RoleResponse>>.Success(roles);
    }

    public async Task<OperationResult<AuthMessageResponse>> AssignAsync(
        AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return OperationResult<AuthMessageResponse>.Failure("User was not found.");
        }

        if (!await _roleManager.RoleExistsAsync(request.RoleName))
        {
            return OperationResult<AuthMessageResponse>.Failure("Role was not found.");
        }

        if (await _userManager.IsInRoleAsync(user, request.RoleName))
        {
            return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("User already has this role."));
        }

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            return OperationResult<AuthMessageResponse>.Failure("Role assignment failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.RoleAssigned, _currentUserService.UserId, nameof(ApplicationUser), user.Id.ToString(), $"Role {request.RoleName} assigned.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Role assigned successfully."));
    }

    public async Task<OperationResult<AuthMessageResponse>> RemoveAsync(
        RemoveRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return OperationResult<AuthMessageResponse>.Failure("User was not found.");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            return OperationResult<AuthMessageResponse>.Failure("Role removal failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.RoleRemoved, _currentUserService.UserId, nameof(ApplicationUser), user.Id.ToString(), $"Role {request.RoleName} removed.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Role removed successfully."));
    }

    public async Task<OperationResult<IReadOnlyList<string>>> GetRolesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return OperationResult<IReadOnlyList<string>>.Failure("User was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return OperationResult<IReadOnlyList<string>>.Success(roles.ToArray());
    }

    private static RoleResponse ToResponse(ApplicationRole role) =>
        new(role.Id, role.Name ?? string.Empty, role.Description);
}
