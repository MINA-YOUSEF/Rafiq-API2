using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MediaId { get; set; }
    public bool IsActive { get; set; } = true;

    public Media Media { get; set; } = null!;
    public ICollection<TreatmentPlanExercise> TreatmentPlanExercises { get; set; } = new List<TreatmentPlanExercise>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
