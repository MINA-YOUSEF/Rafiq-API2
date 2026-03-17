using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Appointments;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class AppointmentsController : BaseV1Controller
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreateAppointmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> Update(
        int id,
        [FromBody] UpdateAppointmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _appointmentService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        await _appointmentService.CompleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("child/{childId:int}")]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> GetByChild(
        int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.GetByChildAsync(childId, request, cancellationToken);
        return Ok(response);
    }
}
