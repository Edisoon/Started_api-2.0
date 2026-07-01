using StartedApi.Application.Users;

namespace StartedApi.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName);

public sealed record ConfirmEmailRequest(
    Guid UserId,
    string Token);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record LogoutRequest(
    string RefreshToken);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record RevokeRefreshTokenRequest(
    string RefreshToken,
    string? Reason = null);

public sealed record ForgotPasswordRequest(
    string Email);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

public sealed record AuthMessageResponse(
    string Message,
    Guid? UserId = null,
    string? ConfirmationToken = null);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    UserProfileResponse User);
