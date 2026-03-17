using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rafiq.Domain.Common;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ParentProfile> ParentProfiles => Set<ParentProfile>();
    public DbSet<SpecialistProfile> SpecialistProfiles => Set<SpecialistProfile>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalReport> MedicalReports => Set<MedicalReport>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseReferenceAnalysis> ExerciseReferenceAnalyses => Set<ExerciseReferenceAnalysis>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentPlanExercise> TreatmentPlanExercises => Set<TreatmentPlanExercise>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionResult> SessionResults => Set<SessionResult>();
    public DbSet<ProgressReport> ProgressReports => Set<ProgressReport>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
