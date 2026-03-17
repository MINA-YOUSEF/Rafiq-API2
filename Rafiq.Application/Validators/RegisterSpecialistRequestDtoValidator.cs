using FluentValidation;
using Rafiq.Application.DTOs.Auth;

namespace Rafiq.Application.Validators;

public class RegisterSpecialistRequestDtoValidator : AbstractValidator<RegisterSpecialistRequestDto>
{
    public RegisterSpecialistRequestDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}
