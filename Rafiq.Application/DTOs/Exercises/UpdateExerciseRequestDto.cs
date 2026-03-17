namespace Rafiq.Application.DTOs.Exercises;

public class UpdateExerciseRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MediaId { get; set; }
    public bool IsActive { get; set; }
}
