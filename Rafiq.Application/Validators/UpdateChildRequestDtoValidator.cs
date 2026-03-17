using FluentValidation;
using Rafiq.Application.DTOs.Children;

namespace Rafiq.Application.Validators;

public class UpdateChildRequestDtoValidator : AbstractValidator<UpdateChildRequestDto>
{
    public UpdateChildRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
