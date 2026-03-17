using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Exercises;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class ExercisesController : BaseV1Controller
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ExerciseDto>>> Get([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var response = await _exerciseService.GetPagedAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> Create([FromBody] CreateExerciseRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _exerciseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { version = "1" }, response);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{exerciseId:int}")]
    public async Task<ActionResult<ExerciseDto>> Update(int exerciseId, [FromBody] UpdateExerciseRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _exerciseService.UpdateAsync(exerciseId, request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{exerciseId:int}")]
    public async Task<IActionResult> Deactivate(int exerciseId, CancellationToken cancellationToken)
    {
        await _exerciseService.DeactivateAsync(exerciseId, cancellationToken);
        return NoContent();
    }
}
