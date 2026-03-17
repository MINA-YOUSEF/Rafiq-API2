using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Admin;
using Rafiq.Application.DTOs.Specialists;

namespace Rafiq.Application.Interfaces.Services;

public interface IAdminService
{
    Task<PagedResult<UserManagementDto>> GetUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<SpecialistListItemDto>> GetSpecialistsAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task SetUserStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default);
    Task AssignRoleAsync(int userId, string role, CancellationToken cancellationToken = default);
    Task<SystemMonitoringDto> GetSystemMonitoringAsync(CancellationToken cancellationToken = default);
}
