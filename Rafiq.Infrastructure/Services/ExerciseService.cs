using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Exercises;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Jobs;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        IBackgroundJobClient backgroundJobClient,
        ILogger<ExerciseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task<PagedResult<ExerciseDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<Exercise> query = _unitOfWork.Exercises.Query().Include(x => x.Media);

        if (!_currentUser.IsInRole(RoleNames.Admin))
        {
            query = query.Where(x => x.IsActive);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ExerciseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ExerciseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequestDto request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        await ValidateExerciseMediaAsync(request.MediaId, cancellationToken);

        var exercise = new Exercise
        {
            Name = request.Name,
            ExerciseType = request.ExerciseType,
            Description = request.Description,
            MediaId = request.MediaId,
            IsActive = true
        };

        await _unitOfWork.Exercises.AddAsync(exercise, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        try
        {
            _backgroundJobClient.Enqueue<ExerciseReferenceExtractionJob>(job => job.ProcessAsync(exercise.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue reference extraction job for Exercise {ExerciseId}.",
                exercise.Id);
        }

        var created = await _unitOfWork.Exercises.Query()
            .Include(x => x.Media)
            .FirstAsync(x => x.Id == exercise.Id, cancellationToken);

        return _mapper.Map<ExerciseDto>(created);
    }

    public async Task<ExerciseDto> UpdateAsync(int exerciseId, UpdateExerciseRequestDto request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId, cancellationToken)
            ?? throw new NotFoundException("Exercise was not found.");

        await ValidateExerciseMediaAsync(request.MediaId, cancellationToken);

        exercise.Name = request.Name;
        exercise.ExerciseType = request.ExerciseType;
        exercise.Description = request.Description;
        exercise.MediaId = request.MediaId;
        exercise.IsActive = request.IsActive;

        _unitOfWork.Exercises.Update(exercise);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Exercises.Query()
            .Include(x => x.Media)
            .FirstAsync(x => x.Id == exercise.Id, cancellationToken);

        return _mapper.Map<ExerciseDto>(updated);
    }

    public async Task DeactivateAsync(int exerciseId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId, cancellationToken)
            ?? throw new NotFoundException("Exercise was not found.");

        exercise.IsActive = false;
        _unitOfWork.Exercises.Update(exercise);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Admin role is required.");
        }
    }

    private async Task ValidateExerciseMediaAsync(int mediaId, CancellationToken cancellationToken)
    {
        var media = await _unitOfWork.Media.GetByIdAsync(mediaId, cancellationToken)
            ?? throw new NotFoundException("Media was not found.");

        if (media.Category != MediaCategory.ExerciseDemo)
        {
            throw new BadRequestException("Exercise media must be in ExerciseDemo category.");
        }
    }
}
