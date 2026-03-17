namespace Rafiq.Application.DTOs.TreatmentPlans;

public class TreatmentPlanExerciseCreateItemDto
{
    public int ExerciseId { get; set; }
    public int ExpectedReps { get; set; }
    public int Sets { get; set; }
    public int DailyFrequency { get; set; }
}
