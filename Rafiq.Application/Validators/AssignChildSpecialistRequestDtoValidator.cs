using FluentValidation;
using Rafiq.Application.DTOs.Children;

namespace Rafiq.Application.Validators;

public class AssignChildSpecialistRequestDtoValidator : AbstractValidator<AssignChildSpecialistRequestDto>
{
    public AssignChildSpecialistRequestDtoValidator()
    {
        RuleFor(x => x.SpecialistProfileId)
            .GreaterThan(0);
    }
}
