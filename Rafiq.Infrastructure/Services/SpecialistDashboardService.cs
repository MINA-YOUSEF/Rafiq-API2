using Microsoft.EntityFrameworkCore;
using Rafiq.Application.DTOs.SpecialistDashboard;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Enums;

namespace Rafiq.Infrastructure.Services;

public class SpecialistDashboardService : ISpecialistDashboardService
{
    private const decimal LowAccuracyThreshold = 60m;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SpecialistDashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SpecialistDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");

        var specialistProfileId = await _unitOfWork.SpecialistProfiles.Query()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (specialistProfileId == 0)
        {
            throw new NotFoundException("Specialist profile not found.");
        }

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var totalAssignedChildren = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(x => x.SpecialistProfileId == specialistProfileId, cancellationToken);

        var totalAppointments = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .CountAsync(x => x.SpecialistUserId == userId, cancellationToken);

        var totalCompletedAppointments = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.SpecialistUserId == userId &&
                     x.Status == AppointmentStatus.Completed,
                cancellationToken);

        var totalSessionsSubmitted = await _unitOfWork.Sessions.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.SubmittedAtUtc.HasValue &&
                     x.Child.SpecialistProfileId == specialistProfileId,
                cancellationToken);

        var averageChildAccuracy = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.SpecialistProfileId == specialistProfileId)
            .Select(x => (decimal?)x.AverageAccuracyScore)
            .AverageAsync(cancellationToken) ?? 0m;

        averageChildAccuracy = Math.Round(averageChildAccuracy, 2, MidpointRounding.AwayFromZero);

        var upcomingAppointments = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .Where(
                x => x.SpecialistUserId == userId &&
                     x.ScheduledAtUtc > now &&
                     (x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Rescheduled))
            .OrderBy(x => x.ScheduledAtUtc)
            .Select(x => new UpcomingAppointmentDto
            {
                AppointmentId = x.Id,
                ChildId = x.ChildId,
                ChildName = x.Child.FullName,
                ScheduledAtUtc = x.ScheduledAtUtc,
                DurationMinutes = x.DurationMinutes,
                Status = x.Status
            })
            .Take(5)
            .ToListAsync(cancellationToken);

        var childrenSnapshot = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.SpecialistProfileId == specialistProfileId)
            .Select(x => new ChildSnapshotDto
            {
                ChildId = x.Id,
                ChildName = x.FullName,
                AnalyzedSessionsCount = x.AnalyzedSessionsCount,
                AverageAccuracyScore = x.AverageAccuracyScore,
                LastSessionDate = _unitOfWork.Sessions.Query()
                    .AsNoTracking()
                    .Where(s => s.ChildId == x.Id && s.SubmittedAtUtc.HasValue)
                    .Max(s => (DateTime?)s.SubmittedAtUtc)
            })
            .OrderByDescending(x => x.LastSessionDate.HasValue)
            .ThenByDescending(x => x.LastSessionDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        var childrenWithoutUpcomingAppointments = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.SpecialistProfileId == specialistProfileId)
            .CountAsync(
                x => !_unitOfWork.Appointments.Query()
                    .AsNoTracking()
                    .Any(a =>
                        a.ChildId == x.Id &&
                        a.SpecialistUserId == userId &&
                        a.ScheduledAtUtc > now &&
                        (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Rescheduled)),
                cancellationToken);

        var childrenWithLowAccuracy = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.SpecialistProfileId == specialistProfileId &&
                     x.AverageAccuracyScore < LowAccuracyThreshold,
                cancellationToken);

        var missedAppointmentsLastMonth = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.SpecialistUserId == userId &&
                     x.ScheduledAtUtc >= lastMonthStart &&
                     x.ScheduledAtUtc < monthStart &&
                     x.Status == AppointmentStatus.Missed,
                cancellationToken);

        return new SpecialistDashboardDto
        {
            Overview = new SpecialistOverviewDto
            {
                TotalAssignedChildren = totalAssignedChildren,
                TotalAppointments = totalAppointments,
                TotalCompletedAppointments = totalCompletedAppointments,
                TotalSessionsSubmitted = totalSessionsSubmitted,
                AverageChildAccuracy = averageChildAccuracy
            },
            UpcomingAppointments = upcomingAppointments,
            ChildrenSnapshot = childrenSnapshot,
            Alerts = new SpecialistAlertsDto
            {
                ChildrenWithoutUpcomingAppointments = childrenWithoutUpcomingAppointments,
                ChildrenWithLowAccuracy = childrenWithLowAccuracy,
                MissedAppointmentsLastMonth = missedAppointmentsLastMonth
            }
        };
    }
}
