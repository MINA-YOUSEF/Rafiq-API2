using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ExerciseType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.MediaId).IsRequired();

        builder.HasOne(x => x.Media)
            .WithOne(x => x.Exercise)
            .HasForeignKey<Exercise>(x => x.MediaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MediaId).IsUnique();
    }
}
