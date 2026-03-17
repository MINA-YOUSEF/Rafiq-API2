using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class SpecialistProfile : BaseEntity
{
    public int UserId { get; set; }
    public int? ProfileImageMediaId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Bio { get; set; }

    public Media? ProfileImage { get; set; }
    public ICollection<Child> AssignedChildren { get; set; } = new List<Child>();
    public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
    public ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();
}
