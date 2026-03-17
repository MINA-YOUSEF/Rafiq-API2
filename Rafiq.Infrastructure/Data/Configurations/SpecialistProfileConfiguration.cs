using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Data.Configurations;

public class SpecialistProfileConfiguration : IEntityTypeConfiguration<SpecialistProfile>
{
    public void Configure(EntityTypeBuilder<SpecialistProfile> builder)
    {
        builder.ToTable("SpecialistProfiles");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Specialization).HasMaxLength(200);
        builder.Property(x => x.Bio).HasMaxLength(2000);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne<AppUser>()
            .WithOne(x => x.SpecialistProfile)
            .HasForeignKey<SpecialistProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProfileImage)
            .WithMany()
            .HasForeignKey(x => x.ProfileImageMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
