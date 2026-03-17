using Rafiq.Application.Common;
using Rafiq.Application.DTOs.TreatmentPlans;

namespace Rafiq.Application.Interfaces.Services;

public interface ITreatmentPlanService
{
    Task<TreatmentPlanDto> CreateAsync(CreateTreatmentPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<TreatmentPlanDto> UpdateAsync(int treatmentPlanId, UpdateTreatmentPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<TreatmentPlanDto> GetByIdAsync(int treatmentPlanId, CancellationToken cancellationToken = default);
    Task<PagedResult<TreatmentPlanDto>> GetByChildAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
    

}

