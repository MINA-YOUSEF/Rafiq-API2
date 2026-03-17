using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class ParentProfile : BaseEntity
{
    public int UserId { get; set; }
    public int? ProfileImageMediaId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public Media? ProfileImage { get; set; }
    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<Session> SessionsStarted { get; set; } = new List<Session>();
}
