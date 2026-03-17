namespace Rafiq.Application.DTOs.Appointments;

public class UpdateAppointmentRequestDto
{
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
}
