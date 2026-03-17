using Rafiq.Domain.Entities;

namespace Rafiq.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IChildRepository Children { get; }
    IGenericRepository<Appointment> Appointments { get; }
    IGenericRepository<ParentProfile> ParentProfiles { get; }
    IGenericRepository<SpecialistProfile> SpecialistProfiles { get; }
    IGenericRepository<MedicalReport> MedicalReports { get; }
    IMediaRepository Media { get; }
    IGenericRepository<Exercise> Exercises { get; }
    IGenericRepository<TreatmentPlan> TreatmentPlans { get; }
    IGenericRepository<TreatmentPlanExercise> TreatmentPlanExercises { get; }
    IGenericRepository<Session> Sessions { get; }
    IGenericRepository<SessionResult> SessionResults { get; }
    IGenericRepository<ProgressReport> ProgressReports { get; }
    IMessageRepository Messages { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
