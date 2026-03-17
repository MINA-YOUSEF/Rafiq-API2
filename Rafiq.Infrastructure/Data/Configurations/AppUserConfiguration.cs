using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.MustChangePassword).HasDefaultValue(false);
    }
}
