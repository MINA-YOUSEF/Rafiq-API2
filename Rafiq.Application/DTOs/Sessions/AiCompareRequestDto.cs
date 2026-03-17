namespace Rafiq.Application.DTOs.Sessions;

public class AiCompareRequestDto
{
    public string ChildVideoUrl { get; set; } = string.Empty;
    public string ReferenceJointAnglesJson { get; set; } = string.Empty;
    public int ReferenceRepetitionCount { get; set; }
}
