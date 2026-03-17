using FluentValidation;
using Rafiq.Application.DTOs.TreatmentPlans;

namespace Rafiq.Application.Validators;

public class TreatmentPlanExerciseCreateItemDtoValidator : AbstractValidator<TreatmentPlanExerciseCreateItemDto>
{
    public TreatmentPlanExerciseCreateItemDtoValidator()
    {
        RuleFor(x => x.ExerciseId).GreaterThan(0);
        RuleFor(x => x.ExpectedReps).GreaterThan(0);
        RuleFor(x => x.Sets).GreaterThan(0);
        RuleFor(x => x.DailyFrequency).GreaterThan(0);
    }
}
