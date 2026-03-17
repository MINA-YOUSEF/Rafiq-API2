using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.TreatmentPlans;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class TreatmentPlanService : ITreatmentPlanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public TreatmentPlanService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<TreatmentPlanDto> CreateAsync(CreateTreatmentPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(request.ChildId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        var specialistProfileId = await ResolveSpecialistProfileIdForWriteAsync(child, cancellationToken);

        await ValidateExerciseIdsAsync(request.Exercises.Select(x => x.ExerciseId), cancellationToken);
        await CheckIsActivePlanAsync(child.Id, null, cancellationToken);
        var treatmentPlan = new TreatmentPlan
        {
            ChildId = child.Id,
            SpecialistProfileId = specialistProfileId,
            Title = request.Title,
            Notes = request.Notes,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        await _unitOfWork.TreatmentPlans.AddAsync(treatmentPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var item in request.Exercises)
        {
            await _unitOfWork.TreatmentPlanExercises.AddAsync(new TreatmentPlanExercise
            {
                TreatmentPlanId = treatmentPlan.Id,
                ExerciseId = item.ExerciseId,
                ExpectedReps = item.ExpectedReps,
                Sets = item.Sets,
                DailyFrequency = item.DailyFrequency
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reloaded = await GetTreatmentPlanWithDetailsAsync(treatmentPlan.Id, cancellationToken);
        return _mapper.Map<TreatmentPlanDto>(reloaded);
    }

    public async Task<TreatmentPlanDto> UpdateAsync(
        int treatmentPlanId,
        UpdateTreatmentPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var treatmentPlan = await _unitOfWork.TreatmentPlans.Query()
            .Include(x => x.Exercises)
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .FirstOrDefaultAsync(x => x.Id == treatmentPlanId, cancellationToken)
            ?? throw new NotFoundException("Treatment plan was not found.");

 await EnsureCanWriteChildAsync(treatmentPlan.Child, cancellationToken);

if (request.IsActive)
{
    await CheckIsActivePlanAsync(treatmentPlan.ChildId, treatmentPlan.Id, cancellationToken);
}        await ValidateExerciseIdsAsync(request.Exercises.Select(x => x.ExerciseId), cancellationToken);

        treatmentPlan.Title = request.Title;
        treatmentPlan.Notes = request.Notes;
        treatmentPlan.StartDate = request.StartDate;
        treatmentPlan.EndDate = request.EndDate;
        treatmentPlan.IsActive = request.IsActive;

        var existingItems = treatmentPlan.Exercises.ToList();
        foreach (var item in existingItems)
        {
            _unitOfWork.TreatmentPlanExercises.Remove(item);
        }

        foreach (var item in request.Exercises)
        {
            await _unitOfWork.TreatmentPlanExercises.AddAsync(new TreatmentPlanExercise
            {
                TreatmentPlanId = treatmentPlan.Id,
                ExerciseId = item.ExerciseId,
                ExpectedReps = item.ExpectedReps,
                Sets = item.Sets,
                DailyFrequency = item.DailyFrequency
            }, cancellationToken);
        }

        _unitOfWork.TreatmentPlans.Update(treatmentPlan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reloaded = await GetTreatmentPlanWithDetailsAsync(treatmentPlan.Id, cancellationToken);
        return _mapper.Map<TreatmentPlanDto>(reloaded);
    }

    public async Task<TreatmentPlanDto> GetByIdAsync(int treatmentPlanId, CancellationToken cancellationToken = default)
    {
        var treatmentPlan = await GetTreatmentPlanWithDetailsAsync(treatmentPlanId, cancellationToken);
        await EnsureCanReadChildAsync(treatmentPlan.Child, cancellationToken);
        return _mapper.Map<TreatmentPlanDto>(treatmentPlan);
    }

    public async Task<PagedResult<TreatmentPlanDto>> GetByChildAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        await EnsureCanReadChildAsync(child, cancellationToken);

        var query = _unitOfWork.TreatmentPlans.Query()
            .Where(x => x.ChildId == childId)
            .Include(x => x.Exercises)
                .ThenInclude(x => x.Exercise);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<TreatmentPlanDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<TreatmentPlanDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private async Task<TreatmentPlan> GetTreatmentPlanWithDetailsAsync(int treatmentPlanId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.TreatmentPlans.Query()
            .Include(x => x.Exercises)
                .ThenInclude(x => x.Exercise)
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .FirstOrDefaultAsync(x => x.Id == treatmentPlanId, cancellationToken)
            ?? throw new NotFoundException("Treatment plan was not found.");
    }

    private async Task ValidateExerciseIdsAsync(IEnumerable<int> exerciseIds, CancellationToken cancellationToken)
    {
        var ids = exerciseIds.Distinct().ToList();
        var count = await _unitOfWork.Exercises.Query()
            .CountAsync(x => ids.Contains(x.Id) && x.IsActive, cancellationToken);

        if (count != ids.Count)
        {
            throw new BadRequestException("One or more exercises are invalid or inactive.");
        }
    }

    private async Task<int> ResolveSpecialistProfileIdForWriteAsync(Child child, CancellationToken cancellationToken)
    {
        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            var userId = GetUserId();
            if (child.SpecialistProfile is null || child.SpecialistProfile.UserId != userId)
            {
                throw new ForbiddenException("Specialist can only manage treatment plans for assigned children.");
            }

            return child.SpecialistProfileId!.Value;
        }

        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            if (!child.SpecialistProfileId.HasValue)
            {
                throw new BadRequestException("Child is not assigned to a specialist.");
            }

            return child.SpecialistProfileId.Value;
        }

        throw new ForbiddenException("Only specialists or admins can create treatment plans.");
    }

    private async Task EnsureCanReadChildAsync(Child child, CancellationToken cancellationToken)
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
private async Task CheckIsActivePlanAsync(
    int childId,
    int? currentPlanId,
    CancellationToken cancellationToken)
{
    var activePlan = await _unitOfWork.TreatmentPlans.Query()
        .Where(x => x.ChildId == childId && x.IsActive)
        .FirstOrDefaultAsync(cancellationToken);

    if (activePlan == null)
        return;

    if (currentPlanId.HasValue && activePlan.Id == currentPlanId.Value)
        return;

    if (activePlan.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
    {
        activePlan.IsActive = false;
        _unitOfWork.TreatmentPlans.Update(activePlan);
        return;
    }

    throw new BadRequestException(
        $"There is already an active treatment plan for this child. PlanId = {activePlan.Id}");
}
    private async Task EnsureCanWriteChildAsync(Child child, CancellationToken cancellationToken)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        var userId = GetUserId();

        if (_currentUser.IsInRole(RoleNames.Specialist) && child.SpecialistProfile?.UserId == userId)
        {
            return;
        }

        throw new ForbiddenException("You are not allowed to modify treatment plans for this child.");
    }

    private int GetUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }
}
