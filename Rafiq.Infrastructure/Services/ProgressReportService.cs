using System.Text.Json;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.ProgressReports;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class ProgressReportService : IProgressReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public ProgressReportService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ProgressReportDto> GenerateAsync(
        GenerateProgressReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(request.ChildId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        EnsureCanGenerate(child);

        var fromUtc = DateTime.SpecifyKind(request.FromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.ToDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var sessions = await _unitOfWork.Sessions.Query()
            .Where(x => x.ChildId == request.ChildId && x.SessionResult != null)
            .Include(x => x.SessionResult)
            .Where(x => x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc <= toUtc)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var sessionFrequency = sessions.Count;
        var accuracyTrend = sessions
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAtUtc))
            .Select(g => new
            {
                Date = g.Key,
                AvgAccuracy = Math.Round(g.Average(x => (double)x.SessionResult!.AccuracyScore), 2)
            })
            .ToList();

        var improvementPercentage = CalculateImprovementPercentage(sessions);

        var specialistProfileId = child.SpecialistProfileId
            ?? throw new BadRequestException("Child is not assigned to a specialist.");

        var report = new ProgressReport
        {
            ChildId = request.ChildId,
            SpecialistProfileId = specialistProfileId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            ImprovementPercentage = improvementPercentage,
            AccuracyTrendsJson = JsonSerializer.Serialize(accuracyTrend),
            SessionFrequency = sessionFrequency,
            Summary = BuildSummary(sessionFrequency, improvementPercentage)
        };

        await _unitOfWork.ProgressReports.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProgressReportDto>(report);
    }

    public async Task<PagedResult<ProgressReportDto>> GetByChildAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        EnsureCanRead(child);

        var query = _unitOfWork.ProgressReports.Query().Where(x => x.ChildId == childId);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ProgressReportDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProgressReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static decimal CalculateImprovementPercentage(IReadOnlyCollection<Session> sessions)
    {
        if (sessions.Count < 2)
        {
            return 0;
        }

        var ordered = sessions.OrderBy(x => x.CreatedAtUtc).ToList();
        var split = ordered.Count / 2;

        if (split == 0 || split == ordered.Count)
        {
            return 0;
        }

        var firstAvg = ordered.Take(split).Average(x => x.SessionResult!.AccuracyScore);
        var secondAvg = ordered.Skip(split).Average(x => x.SessionResult!.AccuracyScore);

        if (firstAvg <= 0)
        {
            return Math.Round(secondAvg, 2);
        }

        return Math.Round(((secondAvg - firstAvg) / firstAvg) * 100, 2);
    }

    private static string BuildSummary(int sessionFrequency, decimal improvementPercentage)
    {
        return $"Completed {sessionFrequency} analyzed sessions in selected range. Improvement: {improvementPercentage}%";
    }

    private void EnsureCanGenerate(Child child)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (!_currentUser.IsInRole(RoleNames.Specialist))
        {
            throw new ForbiddenException("Only specialists or admins can generate progress reports.");
        }

        var userId = GetUserId();
        if (child.SpecialistProfile?.UserId != userId)
        {
            throw new ForbiddenException("Specialist can only generate reports for assigned children.");
        }
    }

    private void EnsureCanRead(Child child)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        var userId = GetUserId();

        if (_currentUser.IsInRole(RoleNames.Parent) && child.ParentProfile.UserId == userId)
        {
            return;
        }

        if (_currentUser.IsInRole(RoleNames.Specialist) && child.SpecialistProfile?.UserId == userId)
        {
            return;
        }

        throw new ForbiddenException("You are not allowed to access this child.");
    }

    private int GetUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }
}
