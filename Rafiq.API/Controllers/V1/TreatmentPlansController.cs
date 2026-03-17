using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.TreatmentPlans;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class TreatmentPlansController : BaseV1Controller
{
    private readonly ITreatmentPlanService _treatmentPlanService;

    public TreatmentPlansController(ITreatmentPlanService treatmentPlanService)
    {
        _treatmentPlanService = treatmentPlanService;
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<TreatmentPlanDto>> Create([FromBody] CreateTreatmentPlanRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _treatmentPlanService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { treatmentPlanId = response.Id, version = "1" }, response);
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPut("{treatmentPlanId:int}")]
    public async Task<ActionResult<TreatmentPlanDto>> Update(
        int treatmentPlanId,
        [FromBody] UpdateTreatmentPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _treatmentPlanService.UpdateAsync(treatmentPlanId, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{treatmentPlanId:int}")]
    public async Task<ActionResult<TreatmentPlanDto>> GetById(int treatmentPlanId, CancellationToken cancellationToken)
    {
        var response = await _treatmentPlanService.GetByIdAsync(treatmentPlanId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("child/{childId:int}")]
    public async Task<ActionResult<PagedResult<TreatmentPlanDto>>> GetByChild(
        int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _treatmentPlanService.GetByChildAsync(childId, request, cancellationToken);
        return Ok(response);
    }
}

