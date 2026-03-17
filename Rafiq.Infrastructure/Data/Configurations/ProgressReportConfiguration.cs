using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class ProgressReportConfiguration : IEntityTypeConfiguration<ProgressReport>
{
    public void Configure(EntityTypeBuilder<ProgressReport> builder)
    {
        builder.ToTable("ProgressReports");
        builder.Property(x => x.FromDate).HasColumnType("date");
        builder.Property(x => x.ToDate).HasColumnType("date");
        builder.Property(x => x.ImprovementPercentage).HasPrecision(6, 2);
        builder.Property(x => x.AccuracyTrendsJson).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.Summary).IsRequired().HasMaxLength(2000);

        builder.HasOne(x => x.Child)
            .WithMany(x => x.ProgressReports)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SpecialistProfile)
            .WithMany(x => x.ProgressReports)
            .HasForeignKey(x => x.SpecialistProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
