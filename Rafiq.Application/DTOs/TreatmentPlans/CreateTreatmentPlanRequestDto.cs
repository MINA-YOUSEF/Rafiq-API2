namespace Rafiq.Application.DTOs.TreatmentPlans;

public class CreateTreatmentPlanRequestDto
{
    public int ChildId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public IReadOnlyCollection<TreatmentPlanExerciseCreateItemDto> Exercises { get; set; } = Array.Empty<TreatmentPlanExerciseCreateItemDto>();
}
