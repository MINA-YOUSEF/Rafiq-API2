namespace Rafiq.Application.DTOs.Specialists;

public class SpecialistListItemDto
{
    public int SpecialistProfileId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? ProfileImageUrl { get; set; }
}
