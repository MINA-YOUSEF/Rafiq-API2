using FluentValidation;
using Rafiq.Application.DTOs.Sessions;

namespace Rafiq.Application.Validators;

public class SubmitSessionVideoRequestDtoValidator : AbstractValidator<SubmitSessionVideoRequestDto>
{
    public SubmitSessionVideoRequestDtoValidator()
    {
        RuleFor(x => x.MediaId).GreaterThan(0);
    }
}
