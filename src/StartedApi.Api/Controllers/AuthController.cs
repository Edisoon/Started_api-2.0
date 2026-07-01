using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedApi.Application.Auth;
using StartedApi.Application.Common;

namespace StartedApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> ConfirmEmail(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("revoke-refresh-token")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> RevokeRefreshToken(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ChangePasswordAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    private ActionResult<ApiResponse<T>> ToActionResult<T>(OperationResult<T> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(ApiResponse<T>.Ok(result.Value, result.Message));
        }

        return BadRequest(ApiResponse<T>.Fail(result.Message, result.Errors));
    }
}
