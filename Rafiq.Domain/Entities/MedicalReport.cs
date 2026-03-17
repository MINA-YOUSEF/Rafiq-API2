using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class MedicalReport : BaseEntity
{
    public int ChildId { get; set; }
    public int MediaId { get; set; }
    public int UploadedByUserId { get; set; }
    public string? Notes { get; set; }

    public Child Child { get; set; } = null!;
    public Media Media { get; set; } = null!;
}
