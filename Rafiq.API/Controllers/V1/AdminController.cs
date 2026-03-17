using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Admin;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize(Policy = "AdminOnly")]
public class AdminController : BaseV1Controller
{
    private readonly IAdminService _adminService;
    private readonly IAuthService _authService;

    public AdminController(IAdminService adminService, IAuthService authService)
    {
        _adminService = adminService;
        _authService = authService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<UserManagementDto>>> GetUsers([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminService.GetUsersAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("users/{userId:int}/status")]
    public async Task<IActionResult> SetUserStatus(int userId, [FromBody] SetUserStatusRequestDto request, CancellationToken cancellationToken)
    {
        await _adminService.SetUserStatusAsync(userId, request.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:int}/roles")]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignRoleRequestDto request, CancellationToken cancellationToken)
    {
        await _adminService.AssignRoleAsync(userId, request.Role, cancellationToken);
        return NoContent();
    }

    [HttpGet("system-monitoring")]
    public async Task<ActionResult<SystemMonitoringDto>> GetSystemMonitoring(CancellationToken cancellationToken)
    {
        var response = await _adminService.GetSystemMonitoringAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("users/{userId:int}/force-reset")]
    public async Task<IActionResult> ForceReset(int userId, CancellationToken cancellationToken)
    {
        await _authService.ForceResetAsync(userId, cancellationToken);
        return NoContent();
    }
}
