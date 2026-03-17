using FluentValidation;
using Rafiq.Application.DTOs.Exercises;

namespace Rafiq.Application.Validators;

public class UpdateExerciseRequestDtoValidator : AbstractValidator<UpdateExerciseRequestDto>
{
    public UpdateExerciseRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ExerciseType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MediaId).GreaterThan(0);
    }
}
