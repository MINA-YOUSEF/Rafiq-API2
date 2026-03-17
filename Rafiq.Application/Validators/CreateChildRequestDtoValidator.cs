using FluentValidation;
using Rafiq.Application.DTOs.Children;

namespace Rafiq.Application.Validators;

public class CreateChildRequestDtoValidator : AbstractValidator<CreateChildRequestDto>
{
    public CreateChildRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
