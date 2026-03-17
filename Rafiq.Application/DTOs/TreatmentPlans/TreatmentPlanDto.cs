namespace Rafiq.Application.DTOs.TreatmentPlans;

public class TreatmentPlanDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int SpecialistProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<TreatmentPlanExerciseDto> Exercises { get; set; } = Array.Empty<TreatmentPlanExerciseDto>();
}
