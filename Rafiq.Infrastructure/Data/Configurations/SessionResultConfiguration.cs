using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Data.Configurations;

public class SessionResultConfiguration : IEntityTypeConfiguration<SessionResult>
{
    public void Configure(EntityTypeBuilder<SessionResult> builder)
    {
        builder.ToTable("SessionResults");
        builder.Property(x => x.AccuracyScore).HasPrecision(5, 2);
        builder.Property(x => x.Feedback).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.JointAnglesJson).IsRequired().HasMaxLength(8000);

        builder.HasOne(x => x.Session)
            .WithOne(x => x.SessionResult)
            .HasForeignKey<SessionResult>(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
