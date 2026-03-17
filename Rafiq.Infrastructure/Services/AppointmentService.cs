using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Appointments;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.External;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        IBackgroundJobClient backgroundJobClient,
        IEmailService emailService,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateSchedule(request.ScheduledAtUtc, request.DurationMinutes);

        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(request.ChildId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        var specialistUserId = ResolveSpecialistUserIdForWrite(child);
        await EnsureNoConflictAsync(null, specialistUserId, request.ScheduledAtUtc, request.DurationMinutes, cancellationToken);

        var appointment = new Appointment
        {
            ChildId = child.Id,
            SpecialistUserId = specialistUserId,
            ScheduledAtUtc = request.ScheduledAtUtc,
            DurationMinutes = request.DurationMinutes,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled
        };

        await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        appointment.ReminderJobId = ScheduleReminderJob(appointment.Id, appointment.ScheduledAtUtc);
        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task<AppointmentDto> UpdateAsync(
        int appointmentId,
        UpdateAppointmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSchedule(request.ScheduledAtUtc, request.DurationMinutes);

        var appointment = await GetAppointmentForWriteAsync(appointmentId, cancellationToken);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.Missed)
        {
            throw new BadRequestException("Only scheduled or rescheduled appointments can be updated.");
        }

        await EnsureNoConflictAsync(
            appointment.Id,
            appointment.SpecialistUserId,
            request.ScheduledAtUtc,
            request.DurationMinutes,
            cancellationToken);

        DeleteReminderJob(appointment.ReminderJobId);

        appointment.ScheduledAtUtc = request.ScheduledAtUtc;
        appointment.DurationMinutes = request.DurationMinutes;
        appointment.Notes = request.Notes;
        appointment.Status = AppointmentStatus.Rescheduled;
        appointment.ReminderJobId = ScheduleReminderJob(appointment.Id, appointment.ScheduledAtUtc);

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task CancelAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await GetAppointmentForWriteAsync(appointmentId, cancellationToken);

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Missed)
        {
            throw new BadRequestException("Completed or missed appointments cannot be cancelled.");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return;
        }

        DeleteReminderJob(appointment.ReminderJobId);

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.ReminderJobId = null;

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await GetAppointmentForWriteAsync(appointmentId, cancellationToken);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Missed)
        {
            throw new BadRequestException("Cancelled or missed appointments cannot be completed.");
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return;
        }

        DeleteReminderJob(appointment.ReminderJobId);

        appointment.Status = AppointmentStatus.Completed;
        appointment.ReminderJobId = null;

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AppointmentDto>> GetByChildAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanReadChildAsync(childId, cancellationToken);

        var query = _unitOfWork.Appointments.Query()
            .Where(x => x.ChildId == childId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.ScheduledAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<AppointmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task AutoMarkMissedAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var appointments = await _unitOfWork.Appointments.Query()
            .Where(x =>
                (x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Rescheduled) &&
                x.ScheduledAtUtc < now)
            .ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            return;
        }

        foreach (var appointment in appointments)
        {
            DeleteReminderJob(appointment.ReminderJobId);
            appointment.ReminderJobId = null;
            appointment.Status = AppointmentStatus.Missed;
            _unitOfWork.Appointments.Update(appointment);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendReminderAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.Query()
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .FirstOrDefaultAsync(x => x.Id == appointmentId, cancellationToken);

        if (appointment is null)
        {
            return;
        }

        if (appointment.Status is not AppointmentStatus.Scheduled and not AppointmentStatus.Rescheduled)
        {
            return;
        }

        var recipientUserIds = new HashSet<int>
        {
            appointment.Child.ParentProfile.UserId,
            appointment.SpecialistUserId
        };

        var recipientEmails = await _userManager.Users
            .Where(x => recipientUserIds.Contains(x.Id) && x.Email != null && x.Email != string.Empty)
            .Select(x => x.Email!)
            .ToListAsync(cancellationToken);

        if (recipientEmails.Count == 0)
        {
            return;
        }

        var subject = "Rafiq Appointment Reminder";
        var body = BuildReminderBody(appointment);

        foreach (var email in recipientEmails.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await _emailService.SendAsync(email, subject, body, cancellationToken);
        }
    }

    private async Task<Appointment> GetAppointmentForWriteAsync(int appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.Query()
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .FirstOrDefaultAsync(x => x.Id == appointmentId, cancellationToken)
            ?? throw new NotFoundException("Appointment was not found.");

        EnsureCanWriteChild(appointment.Child);
        return appointment;
    }

    private async Task EnsureCanReadChildAsync(int childId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken);
            if (child is null)
            {
                throw new NotFoundException("Child was not found.");
            }

            return;
        }

        if (_currentUser.IsInRole(RoleNames.Parent))
        {
            var child = await _unitOfWork.Children.GetByIdForParentAsync(childId, userId, cancellationToken);
            if (child is null)
            {
                throw new NotFoundException("Child was not found or does not belong to current parent.");
            }

            return;
        }

        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            var child = await _unitOfWork.Children.GetByIdForSpecialistAsync(childId, userId, cancellationToken);
            if (child is null)
            {
                throw new NotFoundException("Child was not found or is not assigned to current specialist.");
            }

            return;
        }

        throw new ForbiddenException("You are not allowed to access this child.");
    }

    private int ResolveSpecialistUserIdForWrite(Child child)
    {
        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            var userId = GetUserId();
            if (child.SpecialistProfile?.UserId != userId)
            {
                throw new ForbiddenException("Specialist can only manage appointments for assigned children.");
            }

            return userId;
        }

        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            if (child.SpecialistProfile is null)
            {
                throw new BadRequestException("Child is not assigned to a specialist.");
            }

            return child.SpecialistProfile.UserId;
        }

        throw new ForbiddenException("Only specialists or admins can manage appointments.");
    }

    private void EnsureCanWriteChild(Child child)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            var userId = GetUserId();
            if (child.SpecialistProfile?.UserId == userId)
            {
                return;
            }
        }

        throw new ForbiddenException("You are not allowed to modify appointments for this child.");
    }

    private async Task EnsureNoConflictAsync(
        int? appointmentId,
        int specialistUserId,
        DateTime newStartUtc,
        int durationMinutes,
        CancellationToken cancellationToken)
    {
        var newEndUtc = newStartUtc.AddMinutes(durationMinutes);

        var hasConflict = await _unitOfWork.Appointments.Query()
            .Where(x => x.SpecialistUserId == specialistUserId)
            .Where(x => !appointmentId.HasValue || x.Id != appointmentId.Value)
            .Where(x => x.Status != AppointmentStatus.Cancelled && x.Status != AppointmentStatus.Missed)
            .AnyAsync(
                x => x.ScheduledAtUtc < newEndUtc &&
                     x.ScheduledAtUtc.AddMinutes(x.DurationMinutes) > newStartUtc,
                cancellationToken);

        if (hasConflict)
        {
            throw new BadRequestException("Appointment conflicts with another scheduled appointment for this specialist.");
        }
    }

    private string ScheduleReminderJob(int appointmentId, DateTime scheduledAtUtc)
    {
        var reminderAt = scheduledAtUtc.AddHours(-12);
        if (reminderAt <= DateTime.UtcNow)
        {
            reminderAt = DateTime.UtcNow.AddMinutes(1);
        }

        return _backgroundJobClient.Schedule<IAppointmentService>(
            x => x.SendReminderAsync(appointmentId, CancellationToken.None),
            reminderAt);
    }

    private void DeleteReminderJob(string? reminderJobId)
    {
        if (!string.IsNullOrWhiteSpace(reminderJobId))
        {
            _backgroundJobClient.Delete(reminderJobId);
        }
    }

    private static void ValidateSchedule(DateTime scheduledAtUtc, int durationMinutes)
    {
        if (scheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new BadRequestException("ScheduledAtUtc must be in UTC.");
        }

        if (scheduledAtUtc <= DateTime.UtcNow)
        {
            throw new BadRequestException("Appointment must be scheduled in the future.");
        }

        if (durationMinutes is < 15 or > 240)
        {
            throw new BadRequestException("DurationMinutes must be between 15 and 240.");
        }
    }

    private static string BuildReminderBody(Appointment appointment)
    {
        return
            $"Reminder: Upcoming appointment for {appointment.Child.FullName}{Environment.NewLine}" +
            $"Date: {appointment.ScheduledAtUtc:yyyy-MM-dd}{Environment.NewLine}" +
            $"Time (UTC): {appointment.ScheduledAtUtc:hh:mm tt}{Environment.NewLine}" +
            $"Duration: {appointment.DurationMinutes} minutes";
    }

    private int GetUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }
}
