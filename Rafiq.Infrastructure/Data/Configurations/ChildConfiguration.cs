using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class ChildConfiguration : IEntityTypeConfiguration<Child>
{
    public void Configure(EntityTypeBuilder<Child> builder)
    {
        builder.ToTable("Children");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Diagnosis).HasMaxLength(1000);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.AverageAccuracyScore).HasPrecision(5, 2);

        builder.HasOne(x => x.ParentProfile)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SpecialistProfile)
            .WithMany(x => x.AssignedChildren)
            .HasForeignKey(x => x.SpecialistProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ProfileImage)
            .WithMany()
            .HasForeignKey(x => x.ProfileImageMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
