using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Data;

namespace Rafiq.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    private IChildRepository? _children;
    public IChildRepository Children => _children ??= new ChildRepository(_context);

    private IGenericRepository<Appointment>? _appointments;
    public IGenericRepository<Appointment> Appointments => _appointments ??= new GenericRepository<Appointment>(_context);

    private IGenericRepository<ParentProfile>? _parentProfiles;
    public IGenericRepository<ParentProfile> ParentProfiles => _parentProfiles ??= new GenericRepository<ParentProfile>(_context);

    private IGenericRepository<SpecialistProfile>? _specialistProfiles;
    public IGenericRepository<SpecialistProfile> SpecialistProfiles => _specialistProfiles ??= new GenericRepository<SpecialistProfile>(_context);

    private IGenericRepository<MedicalReport>? _medicalReports;
    public IGenericRepository<MedicalReport> MedicalReports => _medicalReports ??= new GenericRepository<MedicalReport>(_context);

    private IMediaRepository? _media;
    public IMediaRepository Media => _media ??= new MediaRepository(_context);

    private IGenericRepository<Exercise>? _exercises;
    public IGenericRepository<Exercise> Exercises => _exercises ??= new GenericRepository<Exercise>(_context);

    private IGenericRepository<TreatmentPlan>? _treatmentPlans;
    public IGenericRepository<TreatmentPlan> TreatmentPlans => _treatmentPlans ??= new GenericRepository<TreatmentPlan>(_context);

    private IGenericRepository<TreatmentPlanExercise>? _treatmentPlanExercises;
    public IGenericRepository<TreatmentPlanExercise> TreatmentPlanExercises => _treatmentPlanExercises ??= new GenericRepository<TreatmentPlanExercise>(_context);

    private IGenericRepository<Session>? _sessions;
    public IGenericRepository<Session> Sessions => _sessions ??= new GenericRepository<Session>(_context);

    private IGenericRepository<SessionResult>? _sessionResults;
    public IGenericRepository<SessionResult> SessionResults => _sessionResults ??= new GenericRepository<SessionResult>(_context);

    private IGenericRepository<ProgressReport>? _progressReports;
    public IGenericRepository<ProgressReport> ProgressReports => _progressReports ??= new GenericRepository<ProgressReport>(_context);

    private IMessageRepository? _messages;
    public IMessageRepository Messages => _messages ??= new MessageRepository(_context);

    private IGenericRepository<RefreshToken>? _refreshTokens;
    public IGenericRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
