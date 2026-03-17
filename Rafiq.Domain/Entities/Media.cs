using Rafiq.Domain.Common;
using Rafiq.Domain.Enums;

namespace Rafiq.Domain.Entities;

public class Media : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MediaCategory Category { get; set; }
    public int UploadedByUserId { get; set; }
    public int? ChildId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public Child? Child { get; set; }
    public Exercise? Exercise { get; set; }
}
