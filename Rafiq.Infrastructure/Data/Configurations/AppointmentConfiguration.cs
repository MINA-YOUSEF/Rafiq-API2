using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.Property(x => x.ScheduledAtUtc)
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.ReminderJobId)
            .HasMaxLength(100);

        builder.HasOne(x => x.Child)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany(x => x.SpecialistAppointments)
            .HasForeignKey(x => x.SpecialistUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SpecialistUserId);
        builder.HasIndex(x => x.ScheduledAtUtc);
        builder.HasIndex(x => new { x.SpecialistUserId, x.ScheduledAtUtc });
    }
}
