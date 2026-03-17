namespace Rafiq.Application.DTOs.Exercises;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MediaId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string? MediaThumbnailUrl { get; set; }
    public bool IsActive { get; set; }
}
