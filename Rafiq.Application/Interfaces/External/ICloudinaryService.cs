using Rafiq.Application.DTOs.Media;

namespace Rafiq.Application.Interfaces.External;

public interface ICloudinaryService
{
    Task<MediaUploadResult> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<MediaUploadResult> UploadVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<MediaUploadResult> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}
