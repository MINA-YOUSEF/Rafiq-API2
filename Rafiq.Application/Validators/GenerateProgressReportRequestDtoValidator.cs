using FluentValidation;
using Rafiq.Application.DTOs.ProgressReports;

namespace Rafiq.Application.Validators;

public class GenerateProgressReportRequestDtoValidator : AbstractValidator<GenerateProgressReportRequestDto>
{
    public GenerateProgressReportRequestDtoValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0);
        RuleFor(x => x.FromDate).LessThanOrEqualTo(x => x.ToDate);
    }
}
