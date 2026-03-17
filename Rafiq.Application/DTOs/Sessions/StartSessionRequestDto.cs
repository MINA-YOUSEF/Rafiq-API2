namespace Rafiq.Application.DTOs.Sessions;

public class StartSessionRequestDto
{
    public int ChildId { get; set; }
    public int ExerciseId { get; set; }
    public int? TreatmentPlanExerciseId { get; set; }
}
