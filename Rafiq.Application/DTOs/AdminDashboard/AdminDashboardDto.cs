namespace Rafiq.Application.DTOs.AdminDashboard;

public class AdminDashboardDto
{
    public SystemOverviewDto Overview { get; set; } = null!;
    public AppointmentHealthDto AppointmentHealth { get; set; } = null!;
    public MonthlyTrendsDto MonthlyTrends { get; set; } = null!;
    public EngagementMetricsDto Engagement { get; set; } = null!;
    public SmartAlertsDto Alerts { get; set; } = null!;
}

public class SystemOverviewDto
{
    public int TotalParents { get; set; }
    public int TotalSpecialists { get; set; }
    public int TotalChildren { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalMedicalReports { get; set; }
    public int TotalSessions { get; set; }
}

public class AppointmentHealthDto
{
    public int ScheduledThisMonth { get; set; }
    public int CompletedThisMonth { get; set; }
    public int CancelledThisMonth { get; set; }
    public int MissedThisMonth { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal MissedRate { get; set; }
}

public class MonthlyTrendsDto
{
    public IReadOnlyCollection<MonthlyStatDto> ChildrenCreatedLast6Months { get; set; } = Array.Empty<MonthlyStatDto>();
    public IReadOnlyCollection<MonthlyStatDto> AppointmentsCreatedLast6Months { get; set; } = Array.Empty<MonthlyStatDto>();
    public IReadOnlyCollection<MonthlyStatDto> ReportsUploadedLast6Months { get; set; } = Array.Empty<MonthlyStatDto>();
    public IReadOnlyCollection<MonthlyStatDto> SessionsSubmittedLast6Months { get; set; } = Array.Empty<MonthlyStatDto>();
}

public class MonthlyStatDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public class EngagementMetricsDto
{
    public decimal AverageSessionsPerChild { get; set; }
    public decimal AverageReportsPerChild { get; set; }
    public decimal AverageAppointmentsPerSpecialist { get; set; }
}

public class SmartAlertsDto
{
    public int ChildrenWithoutUpcomingAppointments { get; set; }
    public int SpecialistsWithHighMissedRate { get; set; }
    public int UnassignedChildren { get; set; }
    public int ChildrenWithoutReports { get; set; }
}
