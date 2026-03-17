using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rafiq.Application.DTOs.Sessions;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.External;
using Rafiq.Application.Interfaces.Jobs;
using Rafiq.Domain.Entities;
using Rafiq.Domain.Enums;
using Rafiq.Infrastructure.Data;
using Rafiq.Infrastructure.Hubs;

namespace Rafiq.Infrastructure.Jobs;

public class SessionAnalysisJob : ISessionAnalysisJob
{
    private static readonly TimeSpan ReferenceWaitTimeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ReferenceRequeueDelay = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SessionAnalysisJob(
        IServiceScopeFactory scopeFactory,
        IHubContext<ChatHub> hubContext,
        IBackgroundJobClient backgroundJobClient)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _backgroundJobClient = backgroundJobClient;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAsync(int sessionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiClient = scope.ServiceProvider.GetRequiredService<IAiAnalysisClient>();
        Session session;

        try
        {
            var nowUtc = DateTime.UtcNow;

            var rowsAffected = await dbContext.Sessions
                .Where(x => x.Id == sessionId && x.Status == SessionStatus.Submitted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, SessionStatus.Processing)
                    .SetProperty(x => x.UpdatedAtUtc, nowUtc));

            if (rowsAffected == 0)
            {
                await TryFailStaleProcessingSessionAsync(dbContext, sessionId, nowUtc);
                return;
            }

            session = await dbContext.Sessions
                .Include(x => x.Exercise)
                .Include(x => x.TreatmentPlanExercise)
                .Include(x => x.Media)
                .Include(x => x.SessionResult)
                .FirstOrDefaultAsync(x => x.Id == sessionId)
                ?? throw new NotFoundException("Session was not found.");

            if (session.Status == SessionStatus.Analyzed)
            {
                return;
            }

            if (session.Status == SessionStatus.Failed)
            {
                return;
            }

            if (session.Media is null)
            {
                throw new BadRequestException("Session media is missing.");
            }

            var reference = await dbContext.ExerciseReferenceAnalyses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ExerciseId == session.ExerciseId);

            if (reference is null ||
                reference.Status == ExerciseReferenceStatus.Pending ||
                reference.Status == ExerciseReferenceStatus.Processing)
            {
                await HandleReferencePendingOrProcessingAsync(dbContext, session, nowUtc);
                return;
            }

            if (reference.Status == ExerciseReferenceStatus.Failed)
            {
                session.Status = SessionStatus.Failed;
                await dbContext.SaveChangesAsync();
                return;
            }

            session.AnalysisAttempts += 1;
            await dbContext.SaveChangesAsync();

            var aiResponse = await aiClient.CompareAsync(new AiCompareRequestDto
            {
                ChildVideoUrl = session.Media.Url,
                ReferenceJointAnglesJson = reference.ReferenceJointAnglesJson,
                ReferenceRepetitionCount = reference.ReferenceRepetitionCount
            });

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            if (session.SessionResult is null)
            {
                session.SessionResult = new SessionResult
                {
                    SessionId = session.Id,
                    AccuracyScore = aiResponse.AccuracyScore,
                    RepetitionCount = aiResponse.RepetitionCount,
                    MistakeCount = aiResponse.MistakeCount,
                    Feedback = aiResponse.Feedback,
                    JointAnglesJson = aiResponse.JointAngles
                };

                await dbContext.SessionResults.AddAsync(session.SessionResult);
            }
            else
            {
                session.SessionResult.AccuracyScore = aiResponse.AccuracyScore;
                session.SessionResult.RepetitionCount = aiResponse.RepetitionCount;
                session.SessionResult.MistakeCount = aiResponse.MistakeCount;
                session.SessionResult.Feedback = aiResponse.Feedback;
                session.SessionResult.JointAnglesJson = aiResponse.JointAngles;
            }

            session.Status = SessionStatus.Analyzed;

            await UpdateChildProgressSnapshotAsync(dbContext, session.ChildId);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients
                .Group($"child-{session.ChildId}")
                .SendAsync("SessionAnalyzed", session.Id);
        }
        catch
        {
            using var failureScope = _scopeFactory.CreateScope();
            var failureDb = failureScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var failedSession = await failureDb.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId);
            if (failedSession is not null)
            {
                if (failedSession.AnalysisAttempts >= 3)
                {
                    failedSession.Status = SessionStatus.Failed;
                }
                else
                {
                    failedSession.Status = SessionStatus.Submitted;
                }

                await failureDb.SaveChangesAsync();

                if (failedSession.AnalysisAttempts >= 3)
                {
                    return;
                }
            }

            throw;
        }
    }

    private async Task HandleReferencePendingOrProcessingAsync(AppDbContext dbContext, Session session, DateTime nowUtc)
    {
        var submittedAtUtc = session.SubmittedAtUtc ?? session.CreatedAtUtc;
        var waitDuration = nowUtc - submittedAtUtc;

        if (waitDuration > ReferenceWaitTimeout)
        {
            session.Status = SessionStatus.Failed;
            await dbContext.SaveChangesAsync();
            return;
        }

        session.Status = SessionStatus.Submitted;
        await dbContext.SaveChangesAsync();

        try
        {
            _backgroundJobClient.Schedule<ISessionAnalysisJob>(
                job => job.ProcessAsync(session.Id),
                ReferenceRequeueDelay);
        }
        catch
        {
            session.Status = SessionStatus.Failed;
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task TryFailStaleProcessingSessionAsync(AppDbContext dbContext, int sessionId, DateTime nowUtc)
    {
        var staleCandidate = await dbContext.Sessions
            .AsNoTracking()
            .Where(x => x.Id == sessionId && x.Status == SessionStatus.Processing)
            .Select(x => new
            {
                x.SubmittedAtUtc,
                x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (staleCandidate is null)
        {
            return;
        }

        var submittedAtUtc = staleCandidate.SubmittedAtUtc ?? staleCandidate.CreatedAtUtc;
        var waitDuration = nowUtc - submittedAtUtc;
        if (waitDuration <= ReferenceWaitTimeout)
        {
            return;
        }

        await dbContext.Sessions
            .Where(x => x.Id == sessionId && x.Status == SessionStatus.Processing)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, SessionStatus.Failed)
                .SetProperty(x => x.UpdatedAtUtc, nowUtc));
    }

    private static async Task UpdateChildProgressSnapshotAsync(AppDbContext dbContext, int childId)
    {
        var child = await dbContext.Children.FirstOrDefaultAsync(x => x.Id == childId)
            ?? throw new NotFoundException("Child was not found.");

        var analyzedSessions = await dbContext.Sessions
            .Where(x => x.ChildId == childId && x.Status == SessionStatus.Analyzed && x.SessionResult != null)
            .Include(x => x.SessionResult)
            .ToListAsync();

        child.AnalyzedSessionsCount = analyzedSessions.Count;

        child.AverageAccuracyScore = analyzedSessions.Count == 0
            ? 0
            : Math.Round(analyzedSessions.Average(x => x.SessionResult!.AccuracyScore), 2);
    }
}
