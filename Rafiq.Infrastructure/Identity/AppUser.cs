using Microsoft.AspNetCore.Identity;
using Rafiq.Domain.Entities;

namespace Rafiq.Infrastructure.Identity;

public class AppUser : IdentityUser<int>
{
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime? PasswordLastChangedAt { get; set; }
    public ParentProfile? ParentProfile { get; set; }
    public SpecialistProfile? SpecialistProfile { get; set; }
    public ICollection<Appointment> SpecialistAppointments { get; set; } = new List<Appointment>();
}
