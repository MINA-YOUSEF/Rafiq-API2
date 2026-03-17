using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Exercises;

namespace Rafiq.Application.Interfaces.Services;

public interface IExerciseService
{
    Task<PagedResult<ExerciseDto>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ExerciseDto> CreateAsync(CreateExerciseRequestDto request, CancellationToken cancellationToken = default);
    Task<ExerciseDto> UpdateAsync(int exerciseId, UpdateExerciseRequestDto request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int exerciseId, CancellationToken cancellationToken = default);
}
