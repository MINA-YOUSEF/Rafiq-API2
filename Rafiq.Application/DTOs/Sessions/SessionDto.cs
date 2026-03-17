using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.Sessions;

public class SessionDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentProfileId { get; set; }
    public int ExerciseId { get; set; }
    public int? TreatmentPlanExerciseId { get; set; }
    public int? MediaId { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public SessionResultDto? Result { get; set; }
}
