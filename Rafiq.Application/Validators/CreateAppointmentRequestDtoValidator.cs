using FluentValidation;
using Rafiq.Application.DTOs.Appointments;

namespace Rafiq.Application.Validators;

public class CreateAppointmentRequestDtoValidator : AbstractValidator<CreateAppointmentRequestDto>
{
    public CreateAppointmentRequestDtoValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0);
        RuleFor(x => x.ScheduledAtUtc)
            .Must(BeUtc).WithMessage("ScheduledAtUtc must be in UTC.")
            .Must(BeInFuture).WithMessage("ScheduledAtUtc must be in the future.");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(15, 240);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }

    private static bool BeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc;
    }

    private static bool BeInFuture(DateTime value)
    {
        return value > DateTime.UtcNow;
    }
}
