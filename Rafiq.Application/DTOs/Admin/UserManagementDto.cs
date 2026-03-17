namespace Rafiq.Application.DTOs.Admin;

public class UserManagementDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
