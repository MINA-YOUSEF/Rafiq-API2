namespace Rafiq.Application.DTOs.Appointments;

public class CreateAppointmentRequestDto
{
    public int ChildId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
}
