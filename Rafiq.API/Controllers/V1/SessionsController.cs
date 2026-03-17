using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Sessions;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class SessionsController : BaseV1Controller
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [Authorize(Policy = "ParentOnly")]
    [HttpPost("start")]
    public async Task<ActionResult<SessionDto>> Start([FromBody] StartSessionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _sessionService.StartSessionAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "ParentOrAdmin")]
    [HttpPost("{sessionId:int}/submit-video")]
    public async Task<ActionResult<SessionDto>> SubmitVideo(
        int sessionId,
        [FromBody] SubmitSessionVideoRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _sessionService.SubmitSessionVideoAsync(sessionId, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{sessionId:int}")]
    public async Task<ActionResult<SessionDto>> GetById(int sessionId, CancellationToken cancellationToken)
    {
        var response = await _sessionService.GetByIdAsync(sessionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("child/{childId:int}")]
    public async Task<ActionResult<PagedResult<SessionDto>>> GetByChild(
        int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sessionService.GetByChildAsync(childId, request, cancellationToken);
        return Ok(response);
    }
}
