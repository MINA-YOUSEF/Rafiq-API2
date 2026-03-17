using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class SessionResult : BaseEntity
{
    public int SessionId { get; set; }
    public decimal AccuracyScore { get; set; }
    public int RepetitionCount { get; set; }
    public int MistakeCount { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string JointAnglesJson { get; set; } = string.Empty;

    public Session Session { get; set; } = null!;
}
