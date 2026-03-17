namespace Rafiq.Application.DTOs.Sessions;

public class AiAnalysisResponseDto
{
    public decimal AccuracyScore { get; set; }
    public int RepetitionCount { get; set; }
    public int MistakeCount { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string JointAngles { get; set; } = string.Empty;
}
