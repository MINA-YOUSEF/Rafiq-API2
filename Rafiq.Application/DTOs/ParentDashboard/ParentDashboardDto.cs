using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.ParentDashboard;

public class ParentDashboardDto
{
    public ParentOverviewDto Overview { get; set; } = null!;
    public IReadOnlyCollection<ParentChildSnapshotDto> Children { get; set; } = Array.Empty<ParentChildSnapshotDto>();
    public IReadOnlyCollection<ParentUpcomingAppointmentDto> UpcomingAppointments { get; set; } = Array.Empty<ParentUpcomingAppointmentDto>();
    public ParentAlertsDto Alerts { get; set; } = null!;
}

public class ParentOverviewDto
{
    public int TotalChildren { get; set; }
    public int TotalUpcomingAppointments { get; set; }
    public int TotalCompletedSessions { get; set; }
    public decimal AverageAccuracyAcrossChildren { get; set; }
}

public class ParentChildSnapshotDto
{
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string? SpecialistName { get; set; }
    public decimal AverageAccuracyScore { get; set; }
    public int AnalyzedSessionsCount { get; set; }
    public int ReportsCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public DateTime? LastSessionDate { get; set; }
}

public class ParentUpcomingAppointmentDto
{
    public int AppointmentId { get; set; }
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public string? SpecialistName { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
}

public class ParentAlertsDto
{
    public int ChildrenWithoutUpcomingAppointments { get; set; }
    public int ChildrenWithLowAccuracy { get; set; }
}
