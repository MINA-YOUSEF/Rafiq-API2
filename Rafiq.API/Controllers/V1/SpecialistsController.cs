using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Specialists;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize(Roles = "Admin")]
public class SpecialistsController : BaseV1Controller
{
    private readonly IAdminService _adminService;

    public SpecialistsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SpecialistListItemDto>>> GetSpecialists(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _adminService.GetSpecialistsAsync(request, cancellationToken);
        return Ok(response);
    }
}
