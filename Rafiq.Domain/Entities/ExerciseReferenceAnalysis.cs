using Rafiq.Domain.Common;
using Rafiq.Domain.Enums;

namespace Rafiq.Domain.Entities;

public class ExerciseReferenceAnalysis : BaseEntity
{
    public int ExerciseId { get; set; }
    public int MediaId { get; set; }
    public string ReferenceJointAnglesJson { get; set; } = string.Empty;
    public int ReferenceRepetitionCount { get; set; }
    public ExerciseReferenceStatus Status { get; set; } = ExerciseReferenceStatus.Pending;
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public uint RowVersion { get; set; }

    public Exercise Exercise { get; set; } = null!;
    public Media Media { get; set; } = null!;
}
