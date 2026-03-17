using Rafiq.Domain.Enums;

namespace Rafiq.Application.DTOs.Children;

public class CreateChildRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public ChildGender Gender { get; set; }
    public string? Diagnosis { get; set; }
}
