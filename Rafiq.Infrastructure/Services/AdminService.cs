using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Admin;
using Rafiq.Application.DTOs.Specialists;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Infrastructure.Data;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly AppDbContext _dbContext;

    public AdminService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserManagementDto>> GetUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.OrderBy(x => x.Id);
        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<UserManagementDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserManagementDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToArray()
            });
        }

        return new PagedResult<UserManagementDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PagedResult<SpecialistListItemDto>> GetSpecialistsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = from specialistProfile in _dbContext.SpecialistProfiles.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on specialistProfile.UserId equals user.Id
                    join media in _dbContext.Media.AsNoTracking() on specialistProfile.ProfileImageMediaId equals media.Id into profileImageJoin
                    from profileImage in profileImageJoin.DefaultIfEmpty()
                    orderby specialistProfile.Id
                    select new SpecialistListItemDto
                    {
                        SpecialistProfileId = specialistProfile.Id,
                        UserId = specialistProfile.UserId,
                        FullName = specialistProfile.FullName,
                        Email = user.Email ?? string.Empty,
                        Specialization = specialistProfile.Specialization,
                        ProfileImageUrl = profileImage != null && !profileImage.IsDeleted ? profileImage.Url : null
                    };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SpecialistListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task SetUserStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        user.IsActive = isActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }

    public async Task AssignRoleAsync(int userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        var roleExists = await _roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        if (await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }

    public async Task<SystemMonitoringDto> GetSystemMonitoringAsync(CancellationToken cancellationToken = default)
    {
        return new SystemMonitoringDto
        {
            TotalUsers = await _userManager.Users.CountAsync(cancellationToken),
            ActiveUsers = await _userManager.Users.CountAsync(x => x.IsActive, cancellationToken),
            TotalChildren = await _dbContext.Children.CountAsync(cancellationToken),
            TotalSessions = await _dbContext.Sessions.CountAsync(cancellationToken),
            TotalMessages = await _dbContext.Messages.CountAsync(cancellationToken)
        };
    }
}
