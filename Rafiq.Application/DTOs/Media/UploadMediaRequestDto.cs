using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.Media;

public class UploadMediaRequestDto
{
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public MediaCategory Category { get; set; }
    public int? ChildId { get; set; }
}
