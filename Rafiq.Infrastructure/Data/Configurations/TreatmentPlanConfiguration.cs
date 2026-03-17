using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class TreatmentPlanConfiguration : IEntityTypeConfiguration<TreatmentPlan>
{
    public void Configure(EntityTypeBuilder<TreatmentPlan> builder)
    {
        builder.ToTable("TreatmentPlans");
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");

        builder.HasOne(x => x.Child)
            .WithMany(x => x.TreatmentPlans)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SpecialistProfile)
            .WithMany(x => x.TreatmentPlans)
            .HasForeignKey(x => x.SpecialistProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
