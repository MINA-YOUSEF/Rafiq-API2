namespace Rafiq.Application.DTOs.Sessions;

public class AiAnalysisRequestDto
{
    public string VideoUrl { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty;
    public int ExpectedReps { get; set; }
}
