using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.ProgressReports;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class ProgressReportsController : BaseV1Controller
{
    private readonly IProgressReportService _progressReportService;

    public ProgressReportsController(IProgressReportService progressReportService)
    {
        _progressReportService = progressReportService;
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPost("generate")]
    public async Task<ActionResult<ProgressReportDto>> Generate(
        [FromBody] GenerateProgressReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _progressReportService.GenerateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("child/{childId:int}")]
    public async Task<ActionResult<PagedResult<ProgressReportDto>>> GetByChild(
        int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _progressReportService.GetByChildAsync(childId, request, cancellationToken);
        return Ok(response);
    }
}
