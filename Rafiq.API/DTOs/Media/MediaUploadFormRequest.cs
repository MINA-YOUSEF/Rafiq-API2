using Microsoft.AspNetCore.Http;
using Rafiq.Domain.Enums;

namespace Rafiq.API.DTOs.Media;

public class MediaUploadFormRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Description { get; set; }
    public MediaCategory Category { get; set; }
    public int? ChildId { get; set; }
}
