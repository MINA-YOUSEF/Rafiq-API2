namespace Rafiq.Application.DTOs.TreatmentPlans;

public class UpdateTreatmentPlanRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<TreatmentPlanExerciseCreateItemDto> Exercises { get; set; } = Array.Empty<TreatmentPlanExerciseCreateItemDto>();
}
