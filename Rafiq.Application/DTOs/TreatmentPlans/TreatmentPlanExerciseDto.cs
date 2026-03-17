namespace Rafiq.Application.DTOs.TreatmentPlans;

public class TreatmentPlanExerciseDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int ExpectedReps { get; set; }
    public int Sets { get; set; }
    public int DailyFrequency { get; set; }
}
