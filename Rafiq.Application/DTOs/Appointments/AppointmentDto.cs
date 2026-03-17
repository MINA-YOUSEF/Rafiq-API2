using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.Appointments;

public class AppointmentDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int SpecialistUserId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
