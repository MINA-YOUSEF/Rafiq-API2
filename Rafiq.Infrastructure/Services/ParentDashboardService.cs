using Microsoft.EntityFrameworkCore;
using Rafiq.Application.DTOs.ParentDashboard;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Enums;

namespace Rafiq.Infrastructure.Services;

public class ParentDashboardService : IParentDashboardService
{
    private const decimal LowAccuracyThreshold = 60m;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ParentDashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ParentDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");

        var parentProfileId = await _unitOfWork.ParentProfiles.Query()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (parentProfileId == 0)
        {
            throw new NotFoundException("Parent profile not found.");
        }

        var now = DateTime.UtcNow;

        var totalChildren = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(x => x.ParentProfileId == parentProfileId, cancellationToken);

        var totalUpcomingAppointments = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.Child.ParentProfileId == parentProfileId &&
                     x.ScheduledAtUtc > now &&
                     (x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Rescheduled),
                cancellationToken);

        var totalCompletedSessions = await _unitOfWork.Sessions.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.Child.ParentProfileId == parentProfileId &&
                     x.SubmittedAtUtc.HasValue,
                cancellationToken);

        var averageAccuracyAcrossChildren = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.ParentProfileId == parentProfileId)
            .Select(x => (decimal?)x.AverageAccuracyScore)
            .AverageAsync(cancellationToken) ?? 0m;

        averageAccuracyAcrossChildren = Math.Round(averageAccuracyAcrossChildren, 2, MidpointRounding.AwayFromZero);

        var children = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.ParentProfileId == parentProfileId)
            .Select(x => new ParentChildSnapshotDto
            {
                ChildId = x.Id,
                ChildName = x.FullName,
                SpecialistName = x.SpecialistProfile == null ? null : x.SpecialistProfile.FullName,
                AverageAccuracyScore = x.AverageAccuracyScore,
                AnalyzedSessionsCount = x.AnalyzedSessionsCount,
                ReportsCount = _unitOfWork.MedicalReports.Query()
                    .AsNoTracking()
                    .Count(r => r.ChildId == x.Id),
                UpcomingAppointmentsCount = _unitOfWork.Appointments.Query()
                    .AsNoTracking()
                    .Count(a =>
                        a.ChildId == x.Id &&
                        a.ScheduledAtUtc > now &&
                        (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Rescheduled)),
                LastSessionDate = _unitOfWork.Sessions.Query()
                    .AsNoTracking()
                    .Where(s => s.ChildId == x.Id && s.SubmittedAtUtc.HasValue)
                    .Max(s => (DateTime?)s.SubmittedAtUtc)
            })
            .OrderBy(x => x.ChildName)
            .ToListAsync(cancellationToken);

        var upcomingAppointments = await _unitOfWork.Appointments.Query()
            .AsNoTracking()
            .Where(
                x => x.Child.ParentProfileId == parentProfileId &&
                     x.ScheduledAtUtc > now &&
                     (x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Rescheduled))
            .OrderBy(x => x.ScheduledAtUtc)
            .Select(x => new ParentUpcomingAppointmentDto
            {
                AppointmentId = x.Id,
                ChildId = x.ChildId,
                ChildName = x.Child.FullName,
                SpecialistName = _unitOfWork.SpecialistProfiles.Query()
                    .AsNoTracking()
                    .Where(sp => sp.UserId == x.SpecialistUserId)
                    .Select(sp => sp.FullName)
                    .FirstOrDefault(),
                ScheduledAtUtc = x.ScheduledAtUtc,
                DurationMinutes = x.DurationMinutes,
                Status = x.Status
            })
            .Take(5)
            .ToListAsync(cancellationToken);

        var childrenWithoutUpcomingAppointments = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .Where(x => x.ParentProfileId == parentProfileId)
            .CountAsync(
                x => !_unitOfWork.Appointments.Query()
                    .AsNoTracking()
                    .Any(a =>
                        a.ChildId == x.Id &&
                        a.ScheduledAtUtc > now &&
                        (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Rescheduled)),
                cancellationToken);

        var childrenWithLowAccuracy = await _unitOfWork.Children.Query()
            .AsNoTracking()
            .CountAsync(
                x => x.ParentProfileId == parentProfileId &&
                     x.AverageAccuracyScore < LowAccuracyThreshold,
                cancellationToken);

        return new ParentDashboardDto
        {
            Overview = new ParentOverviewDto
            {
                TotalChildren = totalChildren,
                TotalUpcomingAppointments = totalUpcomingAppointments,
                TotalCompletedSessions = totalCompletedSessions,
                AverageAccuracyAcrossChildren = averageAccuracyAcrossChildren
            },
            Children = children,
            UpcomingAppointments = upcomingAppointments,
            Alerts = new ParentAlertsDto
            {
                ChildrenWithoutUpcomingAppointments = childrenWithoutUpcomingAppointments,
                ChildrenWithLowAccuracy = childrenWithLowAccuracy
            }
        };
    }
}
