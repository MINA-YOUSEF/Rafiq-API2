using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Children;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class ChildService : IChildService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<AppUser> _userManager;

    public ChildService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _userManager = userManager;
    }

    public async Task<ChildDto> CreateAsync(CreateChildRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        if (!_currentUser.IsInRole(RoleNames.Parent))
        {
            throw new ForbiddenException("Only parents can create children.");
        }

        var parentProfile = await _unitOfWork.ParentProfiles.Query()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Parent profile not found.");

        var child = new Child
        {
            ParentProfileId = parentProfile.Id,
            SpecialistProfileId = null,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Diagnosis = request.Diagnosis
        };

        await _unitOfWork.Children.AddAsync(child, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ChildDto>(child);
    }

    public async Task<ChildDto> UpdateAsync(int childId, UpdateChildRequestDto request, CancellationToken cancellationToken = default)
    {
        var child = await GetChildForWriteAsync(childId, cancellationToken);

        if (!_currentUser.IsInRole(RoleNames.Admin) && request.SpecialistProfileId != child.SpecialistProfileId)
        {
            throw new ForbiddenException("Only admins can assign or change a specialist.");
        }

        if (_currentUser.IsInRole(RoleNames.Admin) && request.SpecialistProfileId.HasValue)
        {
            _ = await GetActiveSpecialistProfileAsync(request.SpecialistProfileId.Value, cancellationToken);
        }

        child.FullName = request.FullName;
        child.DateOfBirth = request.DateOfBirth;
        child.Gender = request.Gender;
        child.Diagnosis = request.Diagnosis;
        child.SpecialistProfileId = request.SpecialistProfileId;

        _unitOfWork.Children.Update(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ChildDto>(child);
    }

    public async Task AssignSpecialistAsync(int childId, int specialistProfileId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var child = await _unitOfWork.Children.GetByIdAsync(childId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        var specialistProfile = await GetActiveSpecialistProfileAsync(specialistProfileId, cancellationToken);

        if (child.SpecialistProfileId == specialistProfile.Id)
        {
            return;
        }

        child.SpecialistProfileId = specialistProfile.Id;
        _unitOfWork.Children.Update(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnassignSpecialistAsync(int childId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var child = await _unitOfWork.Children.GetByIdAsync(childId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        if (!child.SpecialistProfileId.HasValue)
        {
            return;
        }

        child.SpecialistProfileId = null;
        _unitOfWork.Children.Update(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int childId, CancellationToken cancellationToken = default)
    {
        var child = await GetChildForWriteAsync(childId, cancellationToken);
        _unitOfWork.Children.Remove(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChildDto> GetByIdAsync(int childId, CancellationToken cancellationToken = default)
    {
        var child = await GetChildForReadAsync(childId, cancellationToken);
        return _mapper.Map<ChildDto>(child);
    }

    public async Task<PagedResult<ChildDto>> GetMyChildrenAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        IQueryable<Child> query = _unitOfWork.Children.Query();

        if (_currentUser.IsInRole(RoleNames.Parent))
        {
            query = query.Where(x => x.ParentProfile.UserId == userId);
        }
        else if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            query = query.Where(x => x.SpecialistProfile != null && x.SpecialistProfile.UserId == userId);
        }
        else if (!_currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("You are not allowed to view children.");
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ChildDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ChildDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private async Task<Child> GetChildForReadAsync(int childId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken)
                ?? throw new NotFoundException("Child was not found.");
        }

        if (_currentUser.IsInRole(RoleNames.Parent))
        {
            return await _unitOfWork.Children.GetByIdForParentAsync(childId, userId, cancellationToken)
                ?? throw new NotFoundException("Child was not found or does not belong to current parent.");
        }

        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            return await _unitOfWork.Children.GetByIdForSpecialistAsync(childId, userId, cancellationToken)
                ?? throw new NotFoundException("Child was not found or is not assigned to current specialist.");
        }

        throw new ForbiddenException("You are not allowed to access this child.");
    }

    private async Task<Child> GetChildForWriteAsync(int childId, CancellationToken cancellationToken)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken)
                ?? throw new NotFoundException("Child was not found.");
        }

        if (_currentUser.IsInRole(RoleNames.Parent))
        {
            var userId = GetUserId();
            return await _unitOfWork.Children.GetByIdForParentAsync(childId, userId, cancellationToken)
                ?? throw new NotFoundException("Child was not found or does not belong to current parent.");
        }

        throw new ForbiddenException("You are not allowed to modify this child.");
    }

    private int GetUserId()
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

    private async Task<SpecialistProfile> GetActiveSpecialistProfileAsync(int specialistProfileId, CancellationToken cancellationToken)
    {
        var specialistProfile = await _unitOfWork.SpecialistProfiles.GetByIdAsync(specialistProfileId, cancellationToken)
            ?? throw new NotFoundException("Specialist profile was not found.");

        var specialistUser = await _userManager.FindByIdAsync(specialistProfile.UserId.ToString());
        if (specialistUser is null || !specialistUser.IsActive)
        {
            throw new BadRequestException("Specialist account is inactive.");
        }

        return specialistProfile;
    }
}
