using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Media;

namespace Rafiq.Application.Interfaces.Services;

public interface IMediaService
{
    Task<MediaDto> UploadVideoAsync(UploadMediaRequestDto request, CancellationToken cancellationToken = default);
    Task<MediaDto> UploadImageAsync(UploadMediaRequestDto request, CancellationToken cancellationToken = default);
    Task<MediaDto> UploadFileAsync(UploadMediaRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int mediaId, CancellationToken cancellationToken = default);
    Task<PagedResult<MediaDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<MediaDto>> GetMySessionVideosAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
}
