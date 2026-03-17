using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.DTOs.AdminDashboard;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public AdminDashboardService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonth = monthStart.AddMonths(1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var sixMonthsStart = monthStart.AddMonths(-5);
        var monthKeys = Enumerable.Range(0, 6)
            .Select(i => monthStart.AddMonths(-i))
            .OrderBy(x => x)
            .Select(x => new YearMonth(x.Year, x.Month))
            .ToList();

        var totalParents = await _userManager.Users
            .AsNoTracking()
            .CountAsync(x => x.ParentProfile != null, cancellationToken);

        var totalSpecialists = await _userManager.Users
            .AsNoTracking()
            .CountAsync(x => x.SpecialistProfile != null, cancellationToken);

        var totalChildren = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalAppointments = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalMedicalReports = await _unitOfWork.MedicalReports.Query()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalSessions = await _unitOfWork.Sessions.Query()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var currentMonthStatusCounts = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .Where(x => x.ScheduledAtUtc >= monthStart && x.ScheduledAtUtc < nextMonth)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var scheduledThisMonth = currentMonthStatusCounts
            .Where(x => x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Rescheduled)
            .Sum(x => x.Count);
        var completedThisMonth = currentMonthStatusCounts
            .Where(x => x.Status == AppointmentStatus.Completed)
            .Sum(x => x.Count);
        var cancelledThisMonth = currentMonthStatusCounts
            .Where(x => x.Status == AppointmentStatus.Cancelled)
            .Sum(x => x.Count);
        var missedThisMonth = currentMonthStatusCounts
            .Where(x => x.Status == AppointmentStatus.Missed)
            .Sum(x => x.Count);

        var childrenTrendRaw = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= sixMonthsStart && x.CreatedAtUtc < nextMonth)
            .GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month })
            .Select(g => new MonthlyStatDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var appointmentsTrendRaw = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= sixMonthsStart && x.CreatedAtUtc < nextMonth)
            .GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month })
            .Select(g => new MonthlyStatDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var reportsTrendRaw = await _unitOfWork.MedicalReports.Query()
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= sixMonthsStart && x.CreatedAtUtc < nextMonth)
            .GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month })
            .Select(g => new MonthlyStatDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var sessionsTrendRaw = await _unitOfWork.Sessions.Query()
            .AsNoTracking()
            .Where(x => x.SubmittedAtUtc.HasValue && x.SubmittedAtUtc >= sixMonthsStart && x.SubmittedAtUtc < nextMonth)
            .GroupBy(x => new { x.SubmittedAtUtc!.Value.Year, x.SubmittedAtUtc!.Value.Month })
            .Select(g => new MonthlyStatDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var childrenWithoutUpcomingAppointments = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(
                c => !_unitOfWork.Appointments.Query().AsNoTracking().Any(a =>
                    a.ChildId == c.Id &&
                    a.ScheduledAtUtc > now &&
                    (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Rescheduled)),
                cancellationToken);

        var unassignedChildren = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(x => !x.SpecialistProfileId.HasValue, cancellationToken);

        var childrenWithoutReports = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(
                c => !_unitOfWork.MedicalReports.Query().AsNoTracking().Any(r => r.ChildId == c.Id),
                cancellationToken);

        var specialistsWithHighMissedRate = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .Where(x => x.ScheduledAtUtc >= lastMonthStart && x.ScheduledAtUtc < monthStart)
            .GroupBy(x => x.SpecialistUserId)
            .Select(g => new
            {
                g.Key,
                Total = g.Count(),
                Missed = g.Count(x => x.Status == AppointmentStatus.Missed)
            })
            .CountAsync(x => x.Total > 0 && ((decimal)x.Missed / x.Total) > 0.3m, cancellationToken);

        var currentMonthTotal = scheduledThisMonth + completedThisMonth + cancelledThisMonth + missedThisMonth;
        var completionRate = CalculateRate(completedThisMonth, currentMonthTotal);
        var missedRate = CalculateRate(missedThisMonth, currentMonthTotal);

        var overview = new SystemOverviewDto
        {
            TotalParents = totalParents,
            TotalSpecialists = totalSpecialists,
            TotalChildren = totalChildren,
            TotalAppointments = totalAppointments,
            TotalMedicalReports = totalMedicalReports,
            TotalSessions = totalSessions
        };

        var trends = new MonthlyTrendsDto
        {
            ChildrenCreatedLast6Months = BuildMonthlyStats(monthKeys, childrenTrendRaw),
            AppointmentsCreatedLast6Months = BuildMonthlyStats(monthKeys, appointmentsTrendRaw),
            ReportsUploadedLast6Months = BuildMonthlyStats(monthKeys, reportsTrendRaw),
            SessionsSubmittedLast6Months = BuildMonthlyStats(monthKeys, sessionsTrendRaw)
        };

        var engagement = new EngagementMetricsDto
        {
            AverageSessionsPerChild = CalculateAverage(totalSessions, totalChildren),
            AverageReportsPerChild = CalculateAverage(totalMedicalReports, totalChildren),
            AverageAppointmentsPerSpecialist = CalculateAverage(totalAppointments, totalSpecialists)
        };

        var alerts = new SmartAlertsDto
        {
            ChildrenWithoutUpcomingAppointments = childrenWithoutUpcomingAppointments,
            SpecialistsWithHighMissedRate = specialistsWithHighMissedRate,
            UnassignedChildren = unassignedChildren,
            ChildrenWithoutReports = childrenWithoutReports
        };

        return new AdminDashboardDto
        {
            Overview = overview,
            AppointmentHealth = new AppointmentHealthDto
            {
                ScheduledThisMonth = scheduledThisMonth,
                CompletedThisMonth = completedThisMonth,
                CancelledThisMonth = cancelledThisMonth,
                MissedThisMonth = missedThisMonth,
                CompletionRate = completionRate,
                MissedRate = missedRate
            },
            MonthlyTrends = trends,
            Engagement = engagement,
            Alerts = alerts
        };
    }

    private static IReadOnlyCollection<MonthlyStatDto> BuildMonthlyStats(
        IReadOnlyCollection<YearMonth> keys,
        IReadOnlyCollection<MonthlyStatDto> raw)
    {
        var map = raw.ToDictionary(x => new YearMonth(x.Year, x.Month), x => x.Count);
        return keys
            .Select(k => new MonthlyStatDto
            {
                Year = k.Year,
                Month = k.Month,
                Count = map.TryGetValue(k, out var count) ? count : 0
            })
            .ToList();
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateAverage(int total, int divisor)
    {
        if (divisor == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)total / divisor, 2, MidpointRounding.AwayFromZero);
    }

    private readonly record struct YearMonth(int Year, int Month);
}
