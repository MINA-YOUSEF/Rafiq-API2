using Rafiq.Application.DTOs.ParentDashboard;

namespace Rafiq.Application.Interfaces.Services;

public interface IParentDashboardService
{
    Task<ParentDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
