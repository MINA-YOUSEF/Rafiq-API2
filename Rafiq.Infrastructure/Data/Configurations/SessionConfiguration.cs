using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasOne(x => x.Child)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentProfile)
            .WithMany(x => x.SessionsStarted)
            .HasForeignKey(x => x.ParentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Exercise)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TreatmentPlanExercise)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.TreatmentPlanExerciseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Media)
            .WithMany()
            .HasForeignKey(x => x.MediaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

