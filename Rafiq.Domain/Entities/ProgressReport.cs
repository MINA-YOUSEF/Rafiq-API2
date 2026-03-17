using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class ProgressReport : BaseEntity
{
    public int ChildId { get; set; }
    public int SpecialistProfileId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal ImprovementPercentage { get; set; }
    public string AccuracyTrendsJson { get; set; } = string.Empty;
    public int SessionFrequency { get; set; }
    public string Summary { get; set; } = string.Empty;

    public Child Child { get; set; } = null!;
    public SpecialistProfile SpecialistProfile { get; set; } = null!;
}
