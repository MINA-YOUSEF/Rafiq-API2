using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Appointments;

namespace Rafiq.Application.Interfaces.Services;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequestDto request, CancellationToken cancellationToken = default);
    Task<AppointmentDto> UpdateAsync(int appointmentId, UpdateAppointmentRequestDto request, CancellationToken cancellationToken = default);
    Task CancelAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task CompleteAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<PagedResult<AppointmentDto>> GetByChildAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
    Task AutoMarkMissedAsync(CancellationToken cancellationToken = default);
    Task SendReminderAsync(int appointmentId, CancellationToken cancellationToken = default);
}
