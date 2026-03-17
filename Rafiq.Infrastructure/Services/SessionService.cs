using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Sessions;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Jobs;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SessionService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        IBackgroundJobClient backgroundJobClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<SessionDto> StartSessionAsync(StartSessionRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        if (!_currentUser.IsInRole(RoleNames.Parent))
        {
            throw new ForbiddenException("Only parents can start sessions.");
        }

        var child = await _unitOfWork.Children.GetByIdForParentAsync(request.ChildId, userId, cancellationToken)
            ?? throw new NotFoundException("Child was not found or does not belong to current parent.");

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(request.ExerciseId, cancellationToken)
            ?? throw new NotFoundException("Exercise was not found.");

        if (!exercise.IsActive)
        {
            throw new BadRequestException("Exercise is inactive.");
        }

        if (request.TreatmentPlanExerciseId.HasValue)
        {
            var treatmentPlanExercise = await _unitOfWork.TreatmentPlanExercises.Query()
                .Include(x => x.TreatmentPlan)
                .FirstOrDefaultAsync(x => x.Id == request.TreatmentPlanExerciseId.Value, cancellationToken)
                ?? throw new NotFoundException("Treatment plan exercise was not found.");

            if (treatmentPlanExercise.TreatmentPlan.ChildId != child.Id)
            {
                throw new BadRequestException("The treatment plan exercise does not belong to this child.");
            }
        }

        var session = new Session
        {
            ChildId = child.Id,
            ParentProfileId = child.ParentProfileId,
            ExerciseId = exercise.Id,
            TreatmentPlanExerciseId = request.TreatmentPlanExerciseId,
            Status = SessionStatus.Created
        };

        await _unitOfWork.Sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SessionDto>(session);
    }

    public async Task<SessionDto> SubmitSessionVideoAsync(
        int sessionId,
        SubmitSessionVideoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Sessions.Query()
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .Include(x => x.Media)
            .Include(x => x.TreatmentPlanExercise)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken)
            ?? throw new NotFoundException("Session was not found.");

        EnsureCanWriteSession(session);

        if (session.Status != SessionStatus.Created)
        {
            throw new BadRequestException("Only sessions in Created state can be submitted.");
        }

        var media = await _unitOfWork.Media.GetByIdAsync(request.MediaId, cancellationToken)
            ?? throw new NotFoundException("Media was not found.");

        if (media.Category != MediaCategory.SessionVideo)
        {
            throw new BadRequestException("Session submission requires media of category SessionVideo.");
        }

        var currentUserId = GetUserId();
        if (media.UploadedByUserId != currentUserId)
        {
            throw new ForbiddenException("You are not allowed to submit a session with this media.");
        }

        if (media.ChildId != session.ChildId)
        {
            throw new ForbiddenException("This media does not belong to the session child.");
        }

        session.MediaId = request.MediaId;
        session.SubmittedAtUtc = DateTime.UtcNow;
        session.Status = SessionStatus.Submitted;
        session.AnalysisAttempts = 0;
        _unitOfWork.Sessions.Update(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _backgroundJobClient.Enqueue<ISessionAnalysisJob>(job => job.ProcessAsync(session.Id));

        return _mapper.Map<SessionDto>(session);
    }

    public async Task<SessionDto> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Sessions.Query()
            .Include(x => x.Child)
                .ThenInclude(x => x.ParentProfile)
            .Include(x => x.Child)
                .ThenInclude(x => x.SpecialistProfile)
            .Include(x => x.SessionResult)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken)
            ?? throw new NotFoundException("Session was not found.");

        EnsureCanReadChild(session.Child);
        return _mapper.Map<SessionDto>(session);
    }

    public async Task<PagedResult<SessionDto>> GetByChildAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        EnsureCanReadChild(child);

        var query = _unitOfWork.Sessions.Query()
            .Where(x => x.ChildId == childId)
            .Include(x => x.Media)
            .Include(x => x.SessionResult);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<SessionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<SessionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private void EnsureCanWriteSession(Session session)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        var userId = GetUserId();
        if (_currentUser.IsInRole(RoleNames.Parent) && session.Child.ParentProfile.UserId == userId)
        {
            return;
        }

        throw new ForbiddenException("You are not allowed to modify this session.");
    }

    private void EnsureCanReadChild(Child child)
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
