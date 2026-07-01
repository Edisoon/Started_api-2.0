using StartedApi.Application.Common;

namespace StartedApi.Application.Users;

public interface IUserService
{
    Task<OperationResult<UserProfileResponse>> GetCurrentProfileAsync(
        CancellationToken cancellationToken = default);

    Task<OperationResult<UserProfileResponse>> UpdateCurrentProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<PagedResponse<UserSummaryResponse>>> ListUsersAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<OperationResult<UserDetailResponse>> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<UserDetailResponse>> UpdateStatusAsync(
        Guid userId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default);
}
