using FluentValidation;
using Rafiq.Application.DTOs.Sessions;

namespace Rafiq.Application.Validators;

public class StartSessionRequestDtoValidator : AbstractValidator<StartSessionRequestDto>
{
    public StartSessionRequestDtoValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0);
        RuleFor(x => x.ExerciseId).GreaterThan(0);
    }
}
