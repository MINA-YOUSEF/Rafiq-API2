using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.MedicalReports;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class MedicalReportService : IMedicalReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public MedicalReportService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<MedicalReportDto> CreateAsync(CreateMedicalReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var child = await EnsureCanAccessChildAsync(request.ChildId, cancellationToken);

        var media = await _unitOfWork.Media.GetByIdAsync(request.MediaId, cancellationToken)
            ?? throw new NotFoundException("Media was not found.");

        if (!_currentUser.IsInRole(RoleNames.Admin) && media.UploadedByUserId != userId)
        {
            throw new ForbiddenException("You can only link report media that you uploaded.");
        }

        if (media.Category != MediaCategory.ReportFile)
        {
            throw new BadRequestException("Selected media must be in ReportFile category.");
        }

        if (media.ChildId != request.ChildId)
        {
            throw new BadRequestException("Selected media does not belong to this child.");
        }

        var isAlreadyLinked = await _unitOfWork.MedicalReports.Query()
            .AnyAsync(x => x.MediaId == request.MediaId, cancellationToken);
        if (isAlreadyLinked)
        {
            throw new BadRequestException("This media is already linked to another medical report.");
        }

        var report = new MedicalReport
        {
            ChildId = child.Id,
            MediaId = media.Id,
            UploadedByUserId = userId,
            Notes = request.Notes
        };

        await _unitOfWork.MedicalReports.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MedicalReportDto>(report);
    }

    public async Task<PagedResult<MedicalReportDto>> GetByChildAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanAccessChildAsync(childId, cancellationToken);

        var query = _unitOfWork.MedicalReports.Query().Where(x => x.ChildId == childId);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<MedicalReportDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<MedicalReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task DeleteAsync(int reportId, CancellationToken cancellationToken = default)
    {
        var report = await _unitOfWork.MedicalReports.Query()
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .Include(x => x.Media)
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new NotFoundException("Medical report was not found.");

        var userId = GetUserId();

        if (!_currentUser.IsInRole(RoleNames.Admin) && report.UploadedByUserId != userId)
        {
            throw new ForbiddenException("You are not allowed to delete this medical report.");
        }

        await _unitOfWork.Media.SoftDeleteAsync(report.MediaId, cancellationToken);
        _unitOfWork.MedicalReports.Remove(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetDownloadUrlAsync(int reportId, CancellationToken cancellationToken = default)
    {
        var report = await _unitOfWork.MedicalReports.Query()
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .Include(x => x.Media)
            .FirstOrDefaultAsync(x => x.Id == reportId, cancellationToken)
            ?? throw new NotFoundException("Medical report was not found.");

        await EnsureCanAccessChildAsync(report.ChildId, cancellationToken);

        if (report.Media.Category != MediaCategory.ReportFile)
        {
            throw new BadRequestException("Invalid report media category.");
        }

        if (report.Media.IsDeleted)
        {
            throw new NotFoundException("Medical report file is not available.");
        }

        return report.Media.Url;
    }

    private async Task<Child> EnsureCanAccessChildAsync(int childId, CancellationToken cancellationToken)
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

    private int GetUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }
}
