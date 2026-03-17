using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class TreatmentPlanExerciseConfiguration : IEntityTypeConfiguration<TreatmentPlanExercise>
{
    public void Configure(EntityTypeBuilder<TreatmentPlanExercise> builder)
    {
        builder.ToTable("TreatmentPlanExercises");

        builder.HasOne(x => x.TreatmentPlan)
            .WithMany(x => x.Exercises)
            .HasForeignKey(x => x.TreatmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Exercise)
            .WithMany(x => x.TreatmentPlanExercises)
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TreatmentPlanId, x.ExerciseId }).IsUnique();
    }
}
