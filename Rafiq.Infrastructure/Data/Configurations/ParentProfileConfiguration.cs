using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Data.Configurations;

public class ParentProfileConfiguration : IEntityTypeConfiguration<ParentProfile>
{
    public void Configure(EntityTypeBuilder<ParentProfile> builder)
    {
        builder.ToTable("ParentProfiles");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.Address).HasMaxLength(300);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne<AppUser>()
            .WithOne(x => x.ParentProfile)
            .HasForeignKey<ParentProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProfileImage)
            .WithMany()
            .HasForeignKey(x => x.ProfileImageMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
