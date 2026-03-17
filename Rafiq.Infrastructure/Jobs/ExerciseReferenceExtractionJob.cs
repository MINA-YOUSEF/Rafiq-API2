using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Rafiq.Application.DTOs.Sessions;
using Rafiq.Application.Interfaces.External;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Data;

namespace Rafiq.Infrastructure.Jobs;

public class ExerciseReferenceExtractionJob
{
    private const int MaxAttempts = 3;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExerciseReferenceExtractionJob> _logger;

    public ExerciseReferenceExtractionJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ExerciseReferenceExtractionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAsync(int exerciseId)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiClient = scope.ServiceProvider.GetRequiredService<IAiAnalysisClient>();

        var exercise = await dbContext.Exercises
            .AsNoTracking()
            .Where(x => x.Id == exerciseId)
            .Select(x => new
            {
                x.Id,
                x.MediaId,
                x.ExerciseType,
                MediaUrl = x.Media.Url
            })
            .FirstOrDefaultAsync();

        if (exercise is null)
        {
            _logger.LogWarning("Exercise {ExerciseId} was not found for reference extraction.", exerciseId);
            return;
        }

        var reference = await dbContext.ExerciseReferenceAnalyses
            .FirstOrDefaultAsync(x => x.ExerciseId == exerciseId);

        if (reference is { Status: ExerciseReferenceStatus.Completed })
        {
            return;
        }

        if (reference is { Status: ExerciseReferenceStatus.Processing })
        {
            return;
        }

        if (reference is { Status: ExerciseReferenceStatus.Failed } && reference.Attempts >= MaxAttempts)
        {
            return;
        }

        if (reference is null)
        {
            reference = new ExerciseReferenceAnalysis
            {
                ExerciseId = exerciseId,
                MediaId = exercise.MediaId,
                ReferenceJointAnglesJson = "{}",
                ReferenceRepetitionCount = 0,
                Status = ExerciseReferenceStatus.Pending,
                Attempts = 0
            };

            await dbContext.ExerciseReferenceAnalyses.AddAsync(reference);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _logger.LogInformation(
                    "Reference extraction row already exists for Exercise {ExerciseId}. Skipping duplicate creation.",
                    exerciseId);
                return;
            }
        }

        reference.MediaId = exercise.MediaId;
        reference.Status = ExerciseReferenceStatus.Processing;
        reference.Attempts += 1;
        reference.LastError = null;
        reference.ProcessedAtUtc = null;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogInformation(
                "Concurrency conflict while marking Exercise {ExerciseId} reference as Processing.",
                exerciseId);
            return;
        }

        try
        {
            var extraction = await aiClient.ExtractReferenceAsync(new AiExtractReferenceRequestDto
            {
                VideoUrl = exercise.MediaUrl,
                ExerciseType = exercise.ExerciseType
            });

            reference.ReferenceJointAnglesJson = extraction.JointAngles;
            reference.ReferenceRepetitionCount = extraction.RepetitionCount;
            reference.Status = ExerciseReferenceStatus.Completed;
            reference.ProcessedAtUtc = DateTime.UtcNow;
            reference.LastError = null;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var isTerminal = await MarkFailureAsync(dbContext, reference.Id, ex);
            if (isTerminal)
            {
                _logger.LogError(
                    ex,
                    "Reference extraction failed for Exercise {ExerciseId} and reached terminal retry state.",
                    exerciseId);
                return;
            }

            _logger.LogWarning(ex, "Reference extraction failed for Exercise {ExerciseId}. Retrying.", exerciseId);
            throw;
        }
    }

    private static async Task<bool> MarkFailureAsync(AppDbContext dbContext, int referenceId, Exception exception)
    {
        var reference = await dbContext.ExerciseReferenceAnalyses
            .FirstOrDefaultAsync(x => x.Id == referenceId);

        if (reference is null)
        {
            return true;
        }

        var terminal = reference.Attempts >= MaxAttempts;
        reference.Status = terminal ? ExerciseReferenceStatus.Failed : ExerciseReferenceStatus.Pending;
        reference.LastError = exception.Message.Length > 4000
            ? exception.Message[..4000]
            : exception.Message;
        reference.ProcessedAtUtc = terminal ? DateTime.UtcNow : null;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return true;
        }

        return terminal;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
               postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
