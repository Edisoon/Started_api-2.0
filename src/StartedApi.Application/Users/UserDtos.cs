namespace StartedApi.Application.Users;

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles);

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName);

public sealed record UserQueryParameters(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? Role = null);

public sealed record UserSummaryResponse(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc,
    IReadOnlyList<string> Roles);

public sealed record UserDetailResponse(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? LastLoginAtUtc,
    IReadOnlyList<string> Roles);

public sealed record UpdateUserStatusRequest(
    bool IsActive,
    string? Reason = null);
