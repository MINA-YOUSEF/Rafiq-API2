using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.DTOs.Auth;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

public class AuthController : BaseV1Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register/parent")]
    public async Task<ActionResult<AuthResponseDto>> RegisterParent([FromBody] RegisterParentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterParentAsync(request, cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register/specialist")]
    public async Task<ActionResult<AuthResponseDto>> RegisterSpecialist([FromBody] RegisterSpecialistRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterSpecialistAsync(request, cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }
}
