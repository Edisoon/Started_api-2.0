using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StartedApi.Application.Auth;
using StartedApi.Application.Common;
using StartedApi.Application.Roles;
using StartedApi.Domain.Security;

namespace StartedApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> Create(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleResponse>>>> List(CancellationToken cancellationToken)
    {
        var result = await _roleService.ListAsync(cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> Update(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdateStatus(
        Guid id,
        UpdateRoleStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateStatusAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("assign")]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> Assign(
        AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.AssignAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("remove")]
    public async Task<ActionResult<ApiResponse<AuthMessageResponse>>> Remove(
        RemoveRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.RemoveAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<string>>>> GetUserRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRolesForUserAsync(userId, cancellationToken);
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
