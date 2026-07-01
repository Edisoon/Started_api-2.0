using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StartedApi.Application.Audit;
using StartedApi.Application.Auth;
using StartedApi.Application.Common;
using StartedApi.Application.Security;
using StartedApi.Application.Users;
using StartedApi.Domain.Audit;
using StartedApi.Domain.Auth;
using StartedApi.Domain.Roles;
using StartedApi.Domain.Security;
using StartedApi.Domain.Users;
using StartedApi.Infrastructure.Persistence;

namespace StartedApi.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService,
        IRefreshTokenHasher refreshTokenHasher,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IHostEnvironment hostEnvironment,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _refreshTokenHasher = refreshTokenHasher;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task<OperationResult<AuthMessageResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return OperationResult<AuthMessageResponse>.Failure(
                "Password confirmation does not match.",
                new[] { "Password and confirmation password must match." });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return OperationResult<AuthMessageResponse>.Failure(
                "Registration failed.",
                new[] { "A user with this email already exists." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return OperationResult<AuthMessageResponse>.Failure(
                "Registration failed.",
                result.Errors.Select(error => error.Description).ToArray());
        }

        await EnsureRoleExistsAsync(AppRoles.User);
        await _userManager.AddToRoleAsync(user, AppRoles.User);

        await _auditService.RecordAsync(
            AuditActions.UserUpdated,
            user.Id,
            nameof(ApplicationUser),
            user.Id.ToString(),
            "User registered.",
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        var response = new AuthMessageResponse("User registered successfully. Confirm email before login.");
        if (_hostEnvironment.IsDevelopment())
        {
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation(
                "Development email confirmation token generated for user {UserId} ({Email}): {ConfirmationToken}",
                user.Id,
                user.Email,
                confirmationToken);

            response = response with
            {
                UserId = user.Id,
                ConfirmationToken = confirmationToken
            };
        }

        return OperationResult<AuthMessageResponse>.Success(
            response,
            "User registered successfully.");
    }

    public async Task<OperationResult<AuthMessageResponse>> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return OperationResult<AuthMessageResponse>.Failure("User was not found.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded
            ? OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Email confirmed successfully."))
            : OperationResult<AuthMessageResponse>.Failure("Email confirmation failed.", result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<OperationResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            return OperationResult<AuthResponse>.Failure("Invalid credentials.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
        {
            await _auditService.RecordAsync(AuditActions.AccountLocked, user.Id, nameof(ApplicationUser), user.Id.ToString(), "Account locked.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
            return OperationResult<AuthResponse>.Failure("Account is locked.");
        }

        if (!signInResult.Succeeded)
        {
            return OperationResult<AuthResponse>.Failure("Invalid credentials.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, user.Email!, user.UserName!, roles.ToArray(), cancellationToken);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken.Token);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedByIp = _currentUserService.IpAddress ?? string.Empty
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.RecordAsync(AuditActions.Login, user.Id, nameof(ApplicationUser), user.Id.ToString(), "User logged in.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);

        return OperationResult<AuthResponse>.Success(new AuthResponse(
            accessToken.Token,
            refreshToken.Token,
            accessToken.ExpiresAtUtc,
            ToProfile(user, roles.ToArray())));
    }

    public async Task<OperationResult<AuthMessageResponse>> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await RevokeTokenCoreAsync(request.RefreshToken, "Logout", cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await _auditService.RecordAsync(AuditActions.Logout, _currentUserService.UserId, nameof(RefreshToken), null, "User logged out.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Logout completed."));
    }

    public async Task<OperationResult<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = _refreshTokenHasher.Hash(request.RefreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return OperationResult<AuthResponse>.Failure("Refresh token is invalid.");
        }

        var roles = await _userManager.GetRolesAsync(storedToken.User);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(
            storedToken.User.Id,
            storedToken.User.Email!,
            storedToken.User.UserName!,
            roles.ToArray(),
            cancellationToken);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _refreshTokenHasher.Hash(newRefreshToken.Token);

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.RevokedByIp = _currentUserService.IpAddress;
        storedToken.ReasonRevoked = "Rotated";
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = storedToken.UserId,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
            CreatedByIp = _currentUserService.IpAddress ?? string.Empty
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<AuthResponse>.Success(new AuthResponse(
            accessToken.Token,
            newRefreshToken.Token,
            accessToken.ExpiresAtUtc,
            ToProfile(storedToken.User, roles.ToArray())));
    }

    public Task<OperationResult<AuthMessageResponse>> RevokeRefreshTokenAsync(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken = default) =>
        RevokeTokenCoreAsync(request.RefreshToken, request.Reason ?? "Manual revocation", cancellationToken);

    public async Task<OperationResult<AuthMessageResponse>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            _ = await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        return OperationResult<AuthMessageResponse>.Success(
            new AuthMessageResponse("If the email exists, password reset instructions will be sent."));
    }

    public async Task<OperationResult<AuthMessageResponse>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return OperationResult<AuthMessageResponse>.Failure("Password confirmation does not match.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return OperationResult<AuthMessageResponse>.Failure("Password reset failed.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return OperationResult<AuthMessageResponse>.Failure("Password reset failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.PasswordReset, user.Id, nameof(ApplicationUser), user.Id.ToString(), "Password reset.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Password reset successfully."));
    }

    public async Task<OperationResult<AuthMessageResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return OperationResult<AuthMessageResponse>.Failure("User is not authenticated.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return OperationResult<AuthMessageResponse>.Failure("Password confirmation does not match.");
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user is null)
        {
            return OperationResult<AuthMessageResponse>.Failure("User was not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return OperationResult<AuthMessageResponse>.Failure("Password change failed.", result.Errors.Select(error => error.Description).ToArray());
        }

        await _auditService.RecordAsync(AuditActions.PasswordChanged, user.Id, nameof(ApplicationUser), user.Id.ToString(), "Password changed.", _currentUserService.IpAddress, _currentUserService.UserAgent, cancellationToken);
        return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Password changed successfully."));
    }

    private async Task<OperationResult<AuthMessageResponse>> RevokeTokenCoreAsync(
        string refreshToken,
        string reason,
        CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenHasher.Hash(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return OperationResult<AuthMessageResponse>.Failure("Refresh token is invalid.");
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.RevokedByIp = _currentUserService.IpAddress;
        storedToken.ReasonRevoked = reason;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult<AuthMessageResponse>.Success(new AuthMessageResponse("Refresh token revoked."));
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        await _roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            Description = $"{roleName} role.",
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static UserProfileResponse ToProfile(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, user.FirstName, user.LastName, user.EmailConfirmed, roles);
}
