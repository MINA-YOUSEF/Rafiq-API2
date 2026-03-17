using Microsoft.EntityFrameworkCore;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class ProfileImageService : IProfileImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ProfileImageService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ProfileImageDto> GetParentProfileImageAsync(CancellationToken cancellationToken = default)
    {
        EnsureParentRole();

        var parentProfile = await GetParentProfileAsync(includeImage: true, cancellationToken);
        return MapProfileImage(parentProfile.ProfileImage);
    }

    public async Task SetParentProfileImageAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        EnsureParentRole();

        var userId = GetCurrentUserId();
        var parentProfile = await GetParentProfileAsync(includeImage: false, cancellationToken);
        var media = await GetOwnedProfileImageMediaAsync(mediaId, userId, cancellationToken);

        parentProfile.ProfileImageMediaId = media.Id;
        _unitOfWork.ParentProfiles.Update(parentProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveParentProfileImageAsync(CancellationToken cancellationToken = default)
    {
        EnsureParentRole();

        var parentProfile = await GetParentProfileAsync(includeImage: false, cancellationToken);
        parentProfile.ProfileImageMediaId = null;
        _unitOfWork.ParentProfiles.Update(parentProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProfileImageDto> GetSpecialistProfileImageAsync(CancellationToken cancellationToken = default)
    {
        EnsureSpecialistRole();

        var specialistProfile = await GetSpecialistProfileAsync(includeImage: true, cancellationToken);
        return MapProfileImage(specialistProfile.ProfileImage);
    }

    public async Task SetSpecialistProfileImageAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        EnsureSpecialistRole();

        var userId = GetCurrentUserId();
        var specialistProfile = await GetSpecialistProfileAsync(includeImage: false, cancellationToken);
        var media = await GetOwnedProfileImageMediaAsync(mediaId, userId, cancellationToken);

        specialistProfile.ProfileImageMediaId = media.Id;
        _unitOfWork.SpecialistProfiles.Update(specialistProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSpecialistProfileImageAsync(CancellationToken cancellationToken = default)
    {
        EnsureSpecialistRole();

        var specialistProfile = await GetSpecialistProfileAsync(includeImage: false, cancellationToken);
        specialistProfile.ProfileImageMediaId = null;
        _unitOfWork.SpecialistProfiles.Update(specialistProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProfileImageDto> GetChildProfileImageAsync(int childId, CancellationToken cancellationToken = default)
    {
        var child = await GetChildForReadAsync(childId, includeImage: true, cancellationToken);
        return MapProfileImage(child.ProfileImage);
    }

    public async Task SetChildProfileImageAsync(int childId, int mediaId, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var child = await GetChildForWriteAsync(childId, cancellationToken);
        var media = await GetOwnedProfileImageMediaAsync(mediaId, userId, cancellationToken);

        child.ProfileImageMediaId = media.Id;
        _unitOfWork.Children.Update(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveChildProfileImageAsync(int childId, CancellationToken cancellationToken = default)
    {
        var child = await GetChildForWriteAsync(childId, cancellationToken);
        child.ProfileImageMediaId = null;
        _unitOfWork.Children.Update(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ParentProfile> GetParentProfileAsync(bool includeImage, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = _unitOfWork.ParentProfiles.Query();
        if (includeImage)
        {
            query = query.Include(x => x.ProfileImage);
        }

        return await query.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Parent profile was not found.");
    }

    private async Task<SpecialistProfile> GetSpecialistProfileAsync(bool includeImage, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = _unitOfWork.SpecialistProfiles.Query();
        if (includeImage)
        {
            query = query.Include(x => x.ProfileImage);
        }

        return await query.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Specialist profile was not found.");
    }

    private async Task<Child> GetChildForReadAsync(int childId, bool includeImage, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        IQueryable<Child> query = _unitOfWork.Children.Query()
            .Include(x => x.ParentProfile)
            .Include(x => x.SpecialistProfile);

        if (includeImage)
        {
            query = query.Include(x => x.ProfileImage);
        }

        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return await query.FirstOrDefaultAsync(x => x.Id == childId, cancellationToken)
                ?? throw new NotFoundException("Child was not found.");
        }

        if (_currentUser.IsInRole(RoleNames.Parent))
        {
            return await query.FirstOrDefaultAsync(x => x.Id == childId && x.ParentProfile.UserId == userId, cancellationToken)
                ?? throw new NotFoundException("Child was not found or does not belong to current parent.");
        }

        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            return await query.FirstOrDefaultAsync(
                    x => x.Id == childId &&
                        x.SpecialistProfile != null &&
                        x.SpecialistProfile.UserId == userId,
                    cancellationToken)
                ?? throw new NotFoundException("Child was not found or is not assigned to current specialist.");
        }

        throw new ForbiddenException("You are not allowed to access this child.");
    }

    private async Task<Child> GetChildForWriteAsync(int childId, CancellationToken cancellationToken)
    {
        return await GetChildForReadAsync(childId, includeImage: false, cancellationToken);
    }

    private async Task<Media> GetOwnedProfileImageMediaAsync(int mediaId, int currentUserId, CancellationToken cancellationToken)
    {
        var media = await _unitOfWork.Media.GetByIdAsync(mediaId, cancellationToken)
            ?? throw new NotFoundException("Media was not found.");

        if (media.Category != MediaCategory.ProfileImage)
        {
            throw new BadRequestException("Selected media must be of category ProfileImage.");
        }

        if (media.UploadedByUserId != currentUserId)
        {
            throw new ForbiddenException("You are not allowed to use this media.");
        }

        return media;
    }

    private static ProfileImageDto MapProfileImage(Media? media)
    {
        if (media is null)
        {
            throw new NotFoundException("Profile image was not found.");
        }

        return new ProfileImageDto
        {
            MediaId = media.Id,
            Url = media.Url,
            ThumbnailUrl = media.ThumbnailUrl
        };
    }

    private int GetCurrentUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }

    private void EnsureParentRole()
    {
        if (!_currentUser.IsInRole(RoleNames.Parent))
        {
            throw new ForbiddenException("Parent role is required.");
        }
    }

    private void EnsureSpecialistRole()
    {
        if (!_currentUser.IsInRole(RoleNames.Specialist))
        {
            throw new ForbiddenException("Specialist role is required.");
        }
    }
}
