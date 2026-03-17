using Rafiq.Application.DTOs.SpecialistDashboard;

namespace Rafiq.Application.Interfaces.Services;

public interface ISpecialistDashboardService
{
    Task<SpecialistDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
