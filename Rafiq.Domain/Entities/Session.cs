using Rafiq.Domain.Common;
using Rafiq.Domain.Enums;

namespace Rafiq.Domain.Entities;

public class Session : BaseEntity
{
    public int ChildId { get; set; }
    public int ParentProfileId { get; set; }
    public int ExerciseId { get; set; }
    public int? TreatmentPlanExerciseId { get; set; }
    public int? MediaId { get; set; }
    public int AnalysisAttempts { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Created;
    public DateTime? SubmittedAtUtc { get; set; }

    public Child Child { get; set; } = null!;
    public ParentProfile ParentProfile { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public Media? Media { get; set; }
    public TreatmentPlanExercise? TreatmentPlanExercise { get; set; }
    public SessionResult? SessionResult { get; set; }
}
