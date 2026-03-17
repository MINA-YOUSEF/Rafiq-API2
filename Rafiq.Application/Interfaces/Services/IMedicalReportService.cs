using Rafiq.Application.Common;
using Rafiq.Application.DTOs.MedicalReports;

namespace Rafiq.Application.Interfaces.Services;

public interface IMedicalReportService
{
    Task<MedicalReportDto> CreateAsync(CreateMedicalReportRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResult<MedicalReportDto>> GetByChildAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int reportId, CancellationToken cancellationToken = default);
    Task<string> GetDownloadUrlAsync(int reportId, CancellationToken cancellationToken = default);
}
