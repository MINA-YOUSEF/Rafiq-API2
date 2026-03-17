using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize(Policy = "ParentOnly")]
public class ParentController : BaseV1Controller
{
    private readonly IProfileImageService _profileImageService;

    public ParentController(IProfileImageService profileImageService)
    {
        _profileImageService = profileImageService;
    }

    [HttpGet("profile-image")]
    public async Task<ActionResult<ProfileImageDto>> GetProfileImage(CancellationToken cancellationToken)
    {
        var response = await _profileImageService.GetParentProfileImageAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("profile-image")]
    public async Task<IActionResult> SetProfileImage([FromBody] SetProfileImageRequestDto request, CancellationToken cancellationToken)
    {
        await _profileImageService.SetParentProfileImageAsync(request.MediaId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("profile-image")]
    public async Task<IActionResult> DeleteProfileImage(CancellationToken cancellationToken)
    {
        await _profileImageService.RemoveParentProfileImageAsync(cancellationToken);
        return NoContent();
    }
}
