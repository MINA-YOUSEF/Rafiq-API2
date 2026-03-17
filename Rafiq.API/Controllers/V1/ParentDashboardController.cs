using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.DTOs.ParentDashboard;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "ParentOnly")]
[Route("api/v{version:apiVersion}/parent")]
public class ParentDashboardController : ControllerBase
{
    private readonly IParentDashboardService _parentDashboardService;

    public ParentDashboardController(IParentDashboardService parentDashboardService)
    {
        _parentDashboardService = parentDashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ParentDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var response = await _parentDashboardService.GetDashboardAsync(cancellationToken);
        return Ok(response);
    }
}
