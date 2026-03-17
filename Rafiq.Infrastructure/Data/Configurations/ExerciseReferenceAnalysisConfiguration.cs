using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class ExerciseReferenceAnalysisConfiguration : IEntityTypeConfiguration<ExerciseReferenceAnalysis>
{
    public void Configure(EntityTypeBuilder<ExerciseReferenceAnalysis> builder)
    {
        builder.ToTable("ExerciseReferenceAnalyses");

        builder.Property(x => x.ReferenceJointAnglesJson)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(4000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.ExerciseId)
            .IsUnique();

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Media)
            .WithMany()
            .HasForeignKey(x => x.MediaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
