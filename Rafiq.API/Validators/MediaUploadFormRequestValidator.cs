using FluentValidation;
using Rafiq.API.DTOs.Media;

namespace Rafiq.API.Validators;

public class MediaUploadFormRequestValidator : AbstractValidator<MediaUploadFormRequest>
{
    private const long MaxFileSizeBytes = 200L * 1024 * 1024;

    public MediaUploadFormRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(file => file is { Length: > 0 })
            .WithMessage("File is required.")
            .Must(file => file is null || file.Length <= MaxFileSizeBytes)
            .WithMessage("File size exceeds 200MB limit.");

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.ChildId)
            .GreaterThan(0)
            .When(x => x.ChildId.HasValue);
    }
}
