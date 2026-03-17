using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Children;

namespace Rafiq.Application.Interfaces.Services;

public interface IChildService
{
    Task<ChildDto> CreateAsync(CreateChildRequestDto request, CancellationToken cancellationToken = default);
    Task<ChildDto> UpdateAsync(int childId, UpdateChildRequestDto request, CancellationToken cancellationToken = default);
    Task AssignSpecialistAsync(int childId, int specialistProfileId, CancellationToken cancellationToken = default);
    Task UnassignSpecialistAsync(int childId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int childId, CancellationToken cancellationToken = default);
    Task<ChildDto> GetByIdAsync(int childId, CancellationToken cancellationToken = default);
    Task<PagedResult<ChildDto>> GetMyChildrenAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
