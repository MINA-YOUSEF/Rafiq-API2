using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class TreatmentPlan : BaseEntity
{
    public int ChildId { get; set; }
    public int SpecialistProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Child Child { get; set; } = null!;
    public SpecialistProfile SpecialistProfile { get; set; } = null!;
    public ICollection<TreatmentPlanExercise> Exercises { get; set; } = new List<TreatmentPlanExercise>();
}
