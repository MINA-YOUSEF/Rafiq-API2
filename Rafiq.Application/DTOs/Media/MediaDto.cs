using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.Media;

public class MediaDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MediaCategory Category { get; set; }
    public int UploadedByUserId { get; set; }
    public int? ChildId { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsDeleted { get; set; }
}
