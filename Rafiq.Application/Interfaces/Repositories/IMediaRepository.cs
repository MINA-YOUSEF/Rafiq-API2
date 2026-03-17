using Rafiq.Domain.Entities;

namespace Rafiq.Application.Interfaces.Repositories;

public interface IMediaRepository
{
    Task AddAsync(Media media, CancellationToken cancellationToken = default);
    Task<Media?> GetByIdAsync(int mediaId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<Media> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int mediaId, CancellationToken cancellationToken = default);
    Task<bool> IsLinkedToExerciseAsync(int mediaId, CancellationToken cancellationToken = default);
    Task<bool> IsLinkedToSessionAsync(int mediaId, CancellationToken cancellationToken = default);
    Task<bool> IsLinkedToMedicalReportAsync(int mediaId, CancellationToken cancellationToken = default);
    IQueryable<Media> Query();
}
