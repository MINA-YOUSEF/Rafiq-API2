namespace Rafiq.Application.DTOs.Sessions;

public class SessionResultDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public decimal AccuracyScore { get; set; }
    public int RepetitionCount { get; set; }
    public int MistakeCount { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string JointAnglesJson { get; set; } = string.Empty;
}
