using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedApi.Application.Common;
using StartedApi.Application.Users;
using StartedApi.Domain.Security;

namespace StartedApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _userService.GetCurrentProfileAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> UpdateMe(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateCurrentProfileAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserSummaryResponse>>>> List(
        [FromQuery] UserQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _userService.ListUsersAsync(parameters, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDetailResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<UserDetailResponse>>> UpdateStatus(
        Guid id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateStatusAsync(id, request, cancellationToken);
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
