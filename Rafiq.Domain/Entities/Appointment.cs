using Rafiq.Domain.Common;
using Rafiq.Domain.Enums;

namespace Rafiq.Domain.Entities;

public class Appointment : BaseEntity
{
    public int ChildId { get; set; }
    public int SpecialistUserId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? ReminderJobId { get; set; }

    public Child Child { get; set; } = null!;
}
