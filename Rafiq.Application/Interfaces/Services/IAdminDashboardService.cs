using Rafiq.Application.DTOs.AdminDashboard;

namespace Rafiq.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
