using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.API.DTOs.Media;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class MediaController : BaseV1Controller
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [RequestSizeLimit(100_000_000)]
    [HttpPost("upload/video")]
    public async Task<ActionResult<MediaDto>> UploadVideo([FromForm] MediaUploadFormRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();

        var response = await _mediaService.UploadVideoAsync(new UploadMediaRequestDto
        {
            FileStream = stream,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            Description = request.Description,
            Category = request.Category,
            ChildId = request.ChildId
        }, cancellationToken);

        return Ok(response);
    }

    [RequestSizeLimit(209_715_200)]
    [HttpPost("upload/image")]
    public async Task<ActionResult<MediaDto>> UploadImage([FromForm] MediaUploadFormRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();

        var response = await _mediaService.UploadImageAsync(new UploadMediaRequestDto
        {
            FileStream = stream,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            Description = request.Description,
            Category = request.Category,
            ChildId = request.ChildId
        }, cancellationToken);

        return Ok(response);
    }

    [RequestSizeLimit(209_715_200)]
    [HttpPost("upload")]
    public async Task<ActionResult<MediaDto>> UploadFile([FromForm] MediaUploadFormRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();

        var response = await _mediaService.UploadFileAsync(new UploadMediaRequestDto
        {
            FileStream = stream,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            Description = request.Description,
            Category = request.Category,
            ChildId = request.ChildId
        }, cancellationToken);

        return Ok(response);
    }

    [Authorize(Policy = "SpecialistOrAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediaService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<MediaDto>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediaService.GetPagedAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize(Policy = "ParentOnly")]
    [HttpGet("my-session-videos")]
    public async Task<ActionResult<PagedResult<MediaDto>>> GetMySessionVideos(
        [FromQuery] int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediaService.GetMySessionVideosAsync(childId, request, cancellationToken);
        return Ok(response);
    }
}
