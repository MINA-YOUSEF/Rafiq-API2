using Rafiq.Application.Common;
using Rafiq.Application.DTOs.ProgressReports;

namespace Rafiq.Application.Interfaces.Services;

public interface IProgressReportService
{
    Task<ProgressReportDto> GenerateAsync(GenerateProgressReportRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResult<ProgressReportDto>> GetByChildAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
}
