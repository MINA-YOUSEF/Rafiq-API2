using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.SpecialistDashboard;

public class SpecialistDashboardDto
{
    public SpecialistOverviewDto Overview { get; set; } = null!;
    public IReadOnlyCollection<UpcomingAppointmentDto> UpcomingAppointments { get; set; } = Array.Empty<UpcomingAppointmentDto>();
    public IReadOnlyCollection<ChildSnapshotDto> ChildrenSnapshot { get; set; } = Array.Empty<ChildSnapshotDto>();
    public SpecialistAlertsDto Alerts { get; set; } = null!;
}

public class SpecialistOverviewDto
{
    public int TotalAssignedChildren { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalCompletedAppointments { get; set; }
    public int TotalSessionsSubmitted { get; set; }
    public decimal AverageChildAccuracy { get; set; }
}

public class UpcomingAppointmentDto
{
    public int AppointmentId { get; set; }
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
}

public class ChildSnapshotDto
{
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int AnalyzedSessionsCount { get; set; }
    public decimal AverageAccuracyScore { get; set; }
    public DateTime? LastSessionDate { get; set; }
}

public class SpecialistAlertsDto
{
    public int ChildrenWithoutUpcomingAppointments { get; set; }
    public int ChildrenWithLowAccuracy { get; set; }
    public int MissedAppointmentsLastMonth { get; set; }
}
