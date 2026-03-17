using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.DTOs.SpecialistDashboard;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "SpecialistOnly")]
[Route("api/v{version:apiVersion}/specialist")]
public class SpecialistDashboardController : ControllerBase
{
    private readonly ISpecialistDashboardService _specialistDashboardService;

    public SpecialistDashboardController(ISpecialistDashboardService specialistDashboardService)
    {
        _specialistDashboardService = specialistDashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<SpecialistDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var response = await _specialistDashboardService.GetDashboardAsync(cancellationToken);
        return Ok(response);
    }
}
