using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class TreatmentPlanExercise : BaseEntity
{
    public int TreatmentPlanId { get; set; }
    public int ExerciseId { get; set; }
    public int ExpectedReps { get; set; }
    public int Sets { get; set; }
    public int DailyFrequency { get; set; }

    public TreatmentPlan TreatmentPlan { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
