using Rafiq.Domain.Common;
using Rafiq.Domain.Enums;

namespace Rafiq.Domain.Entities;

public class Child : BaseEntity
{
    public int ParentProfileId { get; set; }
    public int? SpecialistProfileId { get; set; }
    public int? ProfileImageMediaId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public ChildGender Gender { get; set; }
    public string? Diagnosis { get; set; }
    public int AnalyzedSessionsCount { get; set; }
    public decimal AverageAccuracyScore { get; set; }

    public ParentProfile ParentProfile { get; set; } = null!;
    public SpecialistProfile? SpecialistProfile { get; set; }
    public Media? ProfileImage { get; set; }
    public ICollection<MedicalReport> MedicalReports { get; set; } = new List<MedicalReport>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    
}
