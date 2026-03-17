using FluentValidation;
using Rafiq.Application.DTOs.TreatmentPlans;

namespace Rafiq.Application.Validators;

public class CreateTreatmentPlanRequestDtoValidator : AbstractValidator<CreateTreatmentPlanRequestDto>
{
    public CreateTreatmentPlanRequestDtoValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
        RuleFor(x => x.Exercises).NotEmpty();
        RuleForEach(x => x.Exercises).SetValidator(new TreatmentPlanExerciseCreateItemDtoValidator());
    }
}
