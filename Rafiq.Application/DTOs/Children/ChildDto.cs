using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.Children;

public class ChildDto
{
    public int Id { get; set; }
    public int ParentProfileId { get; set; }
    public int? SpecialistProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public ChildGender Gender { get; set; }
    public string? Diagnosis { get; set; }
}
