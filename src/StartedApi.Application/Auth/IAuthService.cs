using StartedApi.Application.Common;

namespace StartedApi.Application.Auth;

public interface IAuthService
{
    Task<OperationResult<AuthMessageResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> RevokeRefreshTokenAsync(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthMessageResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
