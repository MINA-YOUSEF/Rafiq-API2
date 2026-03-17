using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.MedicalReports;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class MedicalReportsController : BaseV1Controller
{
    private readonly IMedicalReportService _medicalReportService;

    public MedicalReportsController(IMedicalReportService medicalReportService)
    {
        _medicalReportService = medicalReportService;
    }

    [HttpPost]
    public async Task<ActionResult<MedicalReportDto>> Create([FromBody] CreateMedicalReportRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _medicalReportService.CreateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("child/{childId:int}")]
    public async Task<ActionResult<PagedResult<MedicalReportDto>>> GetByChild(
        int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _medicalReportService.GetByChildAsync(childId, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _medicalReportService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var url = await _medicalReportService.GetDownloadUrlAsync(id, cancellationToken);
        return Ok(url);
    }
}
