using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartedApi.Application.Audit;
using StartedApi.Application.Common;
using StartedApi.Application.Security;
using StartedApi.Application.Users;
using StartedApi.Domain.Audit;
using StartedApi.Domain.Users;
using StartedApi.Infrastructure.Persistence;

namespace StartedApi.Infrastructure.Users;

public sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<OperationResult<UserProfileResponse>> GetCurrentProfileAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return OperationResult<UserProfileResponse>.Failure("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user is null)
        {
            return OperationResult<UserProfileResponse>.Failure("User was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return OperationResult<UserProfileResponse>.Success(ToProfile(user, roles.ToArray()));
    }

    public async Task<OperationResult<UserProfileResponse>> UpdateCurrentProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return OperationResult<UserProfileResponse>.Failure("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user is null)
        {
            return OperationResult<UserProfileResponse>.Failure("User was not found.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return OperationResult<UserProfileResponse>.Failure("Profile update failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.UserUpdated, user.Id, nameof(ApplicationUser), user.Id.ToString(), "Profile updated.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);
        return OperationResult<UserProfileResponse>.Success(ToProfile(user, roles.ToArray()));
    }

    public async Task<OperationResult<PagedResponse<UserSummaryResponse>>> ListUsersAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(parameters.PageNumber, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var query = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.Trim();
            query = query.Where(user =>
                user.Email!.Contains(searchTerm) ||
                user.FirstName.Contains(searchTerm) ||
                user.LastName.Contains(searchTerm));
        }

        if (parameters.IsActive is not null)
        {
            query = query.Where(user => user.IsActive == parameters.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(user => user.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserSummaryResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrWhiteSpace(parameters.Role) && !roles.Contains(parameters.Role))
            {
                continue;
            }

            items.Add(new UserSummaryResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.EmailConfirmed,
                user.CreatedAtUtc,
                user.LastLoginAtUtc,
                roles.ToArray()));
        }

        return OperationResult<PagedResponse<UserSummaryResponse>>.Success(
            new PagedResponse<UserSummaryResponse>(items, pageNumber, pageSize, totalCount));
    }

    public async Task<OperationResult<UserDetailResponse>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return OperationResult<UserDetailResponse>.Failure("User was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return OperationResult<UserDetailResponse>.Success(ToDetail(user, roles.ToArray()));
    }

    public async Task<OperationResult<UserDetailResponse>> UpdateStatusAsync(
        Guid userId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return OperationResult<UserDetailResponse>.Failure("User was not found.");
        }

        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return OperationResult<UserDetailResponse>.Failure("User status update failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.UserStatusChanged, _currentUserService.UserId, nameof(ApplicationUser), user.Id.ToString(), request.Reason, _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);
        return OperationResult<UserDetailResponse>.Success(ToDetail(user, roles.ToArray()));
    }

    private static UserProfileResponse ToProfile(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.FirstName, user.LastName, user.EmailConfirmed, roles);

    private static UserDetailResponse ToDetail(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.FirstName, user.LastName, user.IsActive, user.EmailConfirmed, user.CreatedAtUtc, user.UpdatedAtUtc, user.LastLoginAtUtc, roles);
}
