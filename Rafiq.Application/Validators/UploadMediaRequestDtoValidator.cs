using FluentValidation;
using Rafiq.Application.DTOs.Media;

namespace Rafiq.Application.Validators;

public class UploadMediaRequestDtoValidator : AbstractValidator<UploadMediaRequestDto>
{
    private const long MaxMediaSizeBytes = 200L * 1024 * 1024;

    public UploadMediaRequestDtoValidator()
    {
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FileSize).GreaterThan(0).LessThanOrEqualTo(MaxMediaSizeBytes);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.ChildId)
            .GreaterThan(0)
            .When(x => x.ChildId.HasValue);
    }
}
