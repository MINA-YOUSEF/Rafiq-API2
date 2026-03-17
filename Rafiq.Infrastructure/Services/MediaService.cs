using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.External;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class MediaService : IMediaService
{
    private const long MaxFileSizeBytes = 200L * 1024 * 1024;
    private const long MaxProfileImageSizeBytes = 5L * 1024 * 1024;

    private static readonly HashSet<string> AllowedVideoMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4",
        "video/quicktime",
        "video/x-msvideo",
        "video/x-matroska",
        "video/webm"
    };

    private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedReportFileMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public MediaService(
        IUnitOfWork unitOfWork,
        ICloudinaryService cloudinaryService,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<MediaDto> UploadVideoAsync(UploadMediaRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Category == MediaCategory.ReportFile)
        {
            throw new BadRequestException("Use the file upload endpoint for report files.");
        }

        ValidateUploadRequest(request, AllowedVideoMimeTypes, MaxFileSizeBytes);
        var currentUserId = GetCurrentUserId();
        var childId = await ResolveAllowedChildIdAsync(request, currentUserId, cancellationToken);

        var uploadResult = await _cloudinaryService.UploadVideoAsync(request.FileStream, request.FileName, cancellationToken);

        var media = new Media
        {
            Url = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            ThumbnailUrl = uploadResult.ThumbnailUrl,
            Description = request.Description,
            Category = request.Category,
            UploadedByUserId = currentUserId,
            ChildId = childId,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Media.AddAsync(media, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MediaDto>(media);
    }

    public async Task<MediaDto> UploadImageAsync(UploadMediaRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Category == MediaCategory.ReportFile)
        {
            throw new BadRequestException("Use the file upload endpoint for report files.");
        }

        var maxSize = request.Category == MediaCategory.ProfileImage
            ? MaxProfileImageSizeBytes
            : MaxFileSizeBytes;

        ValidateUploadRequest(request, AllowedImageMimeTypes, maxSize);
        var currentUserId = GetCurrentUserId();
        var childId = await ResolveAllowedChildIdAsync(request, currentUserId, cancellationToken);

        var uploadResult = await _cloudinaryService.UploadImageAsync(request.FileStream, request.FileName, cancellationToken);

        var media = new Media
        {
            Url = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            ThumbnailUrl = uploadResult.ThumbnailUrl,
            Description = request.Description,
            Category = request.Category,
            UploadedByUserId = currentUserId,
            ChildId = childId,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Media.AddAsync(media, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MediaDto>(media);
    }

    public async Task<MediaDto> UploadFileAsync(UploadMediaRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Category != MediaCategory.ReportFile)
        {
            throw new BadRequestException("Upload endpoint supports ReportFile category only.");
        }

        ValidateReportFileUploadRequest(request);
        var currentUserId = GetCurrentUserId();
        var childId = await ResolveAllowedChildIdAsync(request, currentUserId, cancellationToken);

        var uploadResult = await _cloudinaryService.UploadFileAsync(request.FileStream, request.FileName, cancellationToken);

        var media = new Media
        {
            Url = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            ThumbnailUrl = uploadResult.ThumbnailUrl,
            Description = request.Description,
            Category = request.Category,
            UploadedByUserId = currentUserId,
            ChildId = childId,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Media.AddAsync(media, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MediaDto>(media);
    }

    public async Task DeleteAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Media.GetByIdAsync(mediaId, cancellationToken)
            ?? throw new NotFoundException("Media was not found.");

        var isLinkedToExercise = await _unitOfWork.Media.IsLinkedToExerciseAsync(mediaId, cancellationToken);
        if (isLinkedToExercise)
        {
            throw new BadRequestException("Media cannot be deleted because it is linked to an exercise.");
        }

        var isLinkedToSession = await _unitOfWork.Media.IsLinkedToSessionAsync(mediaId, cancellationToken);
        if (isLinkedToSession)
        {
            throw new BadRequestException("Media is used in a session.");
        }

        var isLinkedToMedicalReport = await _unitOfWork.Media.IsLinkedToMedicalReportAsync(mediaId, cancellationToken);
        if (isLinkedToMedicalReport)
        {
            throw new BadRequestException("Media cannot be deleted because it is linked to a medical report.");
        }

        await _unitOfWork.Media.SoftDeleteAsync(mediaId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<MediaDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var (items, totalCount) = await _unitOfWork.Media.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<MediaDto>
        {
            Items = items.Select(_mapper.Map<MediaDto>).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResult<MediaDto>> GetMySessionVideosAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureParent();

        var currentUserId = GetCurrentUserId();
        var child = await _unitOfWork.Children.GetByIdForParentAsync(childId, currentUserId, cancellationToken);
        if (child is null)
        {
            throw new NotFoundException("Child was not found or does not belong to current parent.");
        }

        var query = _unitOfWork.Media.Query()
            .Where(x =>
                x.UploadedByUserId == currentUserId &&
                x.Category == MediaCategory.SessionVideo &&
                x.ChildId == childId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.UploadedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<MediaDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<MediaDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static void ValidateUploadRequest(
        UploadMediaRequestDto request,
        IReadOnlySet<string> allowedMimeTypes,
        long maxFileSizeBytes)
    {
        if (request.FileStream == Stream.Null)
        {
            throw new BadRequestException("Uploaded file stream is required.");
        }

        if (request.FileSize <= 0)
        {
            throw new BadRequestException("Uploaded file is empty.");
        }

        if (request.FileSize > maxFileSizeBytes)
        {
            throw new BadRequestException($"File size exceeds {maxFileSizeBytes / (1024 * 1024)}MB limit.");
        }

        if (!allowedMimeTypes.Contains(request.ContentType))
        {
            throw new BadRequestException("Unsupported media MIME type.");
        }
    }

    private static void ValidateReportFileUploadRequest(UploadMediaRequestDto request)
    {
        ValidateUploadRequest(request, AllowedReportFileMimeTypes, MaxFileSizeBytes);

        var extension = Path.GetExtension(request.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Only .pdf files are allowed for report uploads.");
        }
    }

    private async Task<int?> ResolveAllowedChildIdAsync(
        UploadMediaRequestDto request,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        if (request.Category == MediaCategory.ProfileImage)
        {
            if (!_currentUser.IsInRole(RoleNames.Parent) && !_currentUser.IsInRole(RoleNames.Specialist))
            {
                throw new ForbiddenException("Only parents or specialists can upload profile images.");
            }

            if (request.ChildId.HasValue)
            {
                throw new BadRequestException("ChildId must be null when category is ProfileImage.");
            }

            return null;
        }

        if (request.Category == MediaCategory.SessionVideo)
        {
            if (!_currentUser.IsInRole(RoleNames.Parent))
            {
                throw new ForbiddenException("Only parents can upload session videos.");
            }

            if (!request.ChildId.HasValue)
            {
                throw new BadRequestException("ChildId is required for session video uploads.");
            }

            var child = await _unitOfWork.Children.GetByIdForParentAsync(request.ChildId.Value, currentUserId, cancellationToken);
            if (child is null)
            {
                throw new ForbiddenException("You are not allowed to upload media for this child.");
            }

            return child.Id;
        }

        if (request.Category == MediaCategory.ReportFile)
        {
            if (!request.ChildId.HasValue)
            {
                throw new BadRequestException("ChildId is required when category is ReportFile.");
            }

            if (_currentUser.IsInRole(RoleNames.Admin))
            {
                var exists = await _unitOfWork.Children.Query()
                    .AnyAsync(x => x.Id == request.ChildId.Value, cancellationToken);
                if (!exists)
                {
                    throw new NotFoundException("Child was not found.");
                }

                return request.ChildId.Value;
            }

            if (_currentUser.IsInRole(RoleNames.Parent))
            {
                var child = await _unitOfWork.Children.GetByIdForParentAsync(
                    request.ChildId.Value,
                    currentUserId,
                    cancellationToken);
                if (child is null)
                {
                    throw new NotFoundException("Child was not found or does not belong to current parent.");
                }

                return child.Id;
            }

            if (_currentUser.IsInRole(RoleNames.Specialist))
            {
                var child = await _unitOfWork.Children.GetByIdForSpecialistAsync(
                    request.ChildId.Value,
                    currentUserId,
                    cancellationToken);
                if (child is null)
                {
                    throw new NotFoundException("Child was not found or is not assigned to current specialist.");
                }

                return child.Id;
            }

            throw new ForbiddenException("You are not allowed to upload report files.");
        }

        if (request.ChildId.HasValue)
        {
            throw new BadRequestException("ChildId can only be set when category is SessionVideo.");
        }

        if (request.Category == MediaCategory.ExerciseDemo &&
            !_currentUser.IsInRole(RoleNames.Specialist) &&
            !_currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only specialists or admins can upload exercise demo media.");
        }

        if (!_currentUser.IsInRole(RoleNames.Specialist) &&
            !_currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only specialists or admins can upload this media category.");
        }

        return null;
    }

    private int GetCurrentUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Admin role is required.");
        }
    }

    private void EnsureParent()
    {
        if (!_currentUser.IsInRole(RoleNames.Parent))
        {
            throw new ForbiddenException("Parent role is required.");
        }
    }
}
