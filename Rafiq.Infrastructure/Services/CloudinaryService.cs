using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.External;
using Rafiq.Infrastructure.Options;

namespace Rafiq.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinaryOptions> options)
    {
        var config = options.Value;
        var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<MediaUploadResult> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "rafiq/images"
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.AbsoluteUri))
        {
            throw new BadRequestException(result.Error?.Message ?? "Cloudinary image upload failed.");
        }

        return new MediaUploadResult
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            ThumbnailUrl = result.SecureUrl.AbsoluteUri
        };
    }

    public async Task<MediaUploadResult> UploadVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "rafiq/videos"
        };

        var result = await _cloudinary.UploadLargeAsync(uploadParams, 6_000_000, cancellationToken);
        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.AbsoluteUri))
        {
            throw new BadRequestException(result.Error?.Message ?? "Cloudinary video upload failed.");
        }

        var thumbnailUrl = _cloudinary.Api.UrlVideoUp
            .Transform(new Transformation()
                .Width(640)
                .Height(360)
                .Crop("fill")
                .Gravity("auto")
                .FetchFormat("jpg")
                .Quality("auto"))
            .BuildUrl(result.PublicId);

        return new MediaUploadResult
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            ThumbnailUrl = thumbnailUrl
        };
    }

    public async Task<MediaUploadResult> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "rafiq/files"
        };

        var result = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);
        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.AbsoluteUri))
        {
            throw new BadRequestException(result.Error?.Message ?? "Cloudinary file upload failed.");
        }

        return new MediaUploadResult
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            ThumbnailUrl = null
        };
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var imageDelete = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        });

        if (imageDelete.Error is null && IsDeleted(imageDelete.Result))
        {
            return;
        }

        var videoDelete = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Video
        });

        if (videoDelete.Error is not null)
        {
            throw new BadRequestException(videoDelete.Error.Message);
        }
    }

    private static bool IsDeleted(string? result)
    {
        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase)
            || string.Equals(result, "not found", StringComparison.OrdinalIgnoreCase);
    }
}
