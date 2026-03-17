using FluentValidation;
using Rafiq.Application.DTOs.MedicalReports;

namespace Rafiq.Application.Validators;

public class CreateMedicalReportRequestDtoValidator : AbstractValidator<CreateMedicalReportRequestDto>
{
    public CreateMedicalReportRequestDtoValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0);
        RuleFor(x => x.MediaId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
