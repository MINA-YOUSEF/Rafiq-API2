using FluentValidation;
using Rafiq.Application.DTOs.Media;

namespace Rafiq.Application.Validators;

public class SetProfileImageRequestDtoValidator : AbstractValidator<SetProfileImageRequestDto>
{
    public SetProfileImageRequestDtoValidator()
    {
        RuleFor(x => x.MediaId)
            .GreaterThan(0);
    }
}
