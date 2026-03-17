using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Children;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class ChildrenController : BaseV1Controller
{
    private readonly IChildService _childService;
    private readonly IProfileImageService _profileImageService;

    public ChildrenController(IChildService childService, IProfileImageService profileImageService)
    {
        _childService = childService;
        _profileImageService = profileImageService;
    }

    [HttpGet]
        public async Task<ActionResult<PagedResult<ChildDto>>> GetMyChildren([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var response = await _childService.GetMyChildrenAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{childId:int}")]
    public async Task<ActionResult<ChildDto>> GetById(int childId, CancellationToken cancellationToken)
    {
        var response = await _childService.GetByIdAsync(childId, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "ParentOnly")]
    [HttpPost]
    public async Task<ActionResult<ChildDto>> Create([FromBody] CreateChildRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _childService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { childId = response.Id, version = "1" }, response);
    }

    [Authorize(Policy = "ParentOrAdmin")]
    [HttpPut("{childId:int}")]
    public async Task<ActionResult<ChildDto>> Update(int childId, [FromBody] UpdateChildRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _childService.UpdateAsync(childId, request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "ParentOrAdmin")]
    [HttpDelete("{childId:int}")]
    public async Task<IActionResult> Delete(int childId, CancellationToken cancellationToken)
    {
        await _childService.DeleteAsync(childId, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{childId:int}/specialist")]
    public async Task<IActionResult> AssignSpecialist(
        int childId,
        [FromBody] AssignChildSpecialistRequestDto request,
        CancellationToken cancellationToken)
    {
        await _childService.AssignSpecialistAsync(childId, request.SpecialistProfileId, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{childId:int}/specialist")]
    public async Task<IActionResult> UnassignSpecialist(int childId, CancellationToken cancellationToken)
    {
        await _childService.UnassignSpecialistAsync(childId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{childId:int}/profile-image")]
    public async Task<ActionResult<ProfileImageDto>> GetProfileImage(int childId, CancellationToken cancellationToken)
    {
        var response = await _profileImageService.GetChildProfileImageAsync(childId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{childId:int}/profile-image")]
    public async Task<IActionResult> SetProfileImage(
        int childId,
        [FromBody] SetProfileImageRequestDto request,
        CancellationToken cancellationToken)
    {
        await _profileImageService.SetChildProfileImageAsync(childId, request.MediaId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{childId:int}/profile-image")]
    public async Task<IActionResult> DeleteProfileImage(int childId, CancellationToken cancellationToken)
    {
        await _profileImageService.RemoveChildProfileImageAsync(childId, cancellationToken);
        return NoContent();
    }
}
