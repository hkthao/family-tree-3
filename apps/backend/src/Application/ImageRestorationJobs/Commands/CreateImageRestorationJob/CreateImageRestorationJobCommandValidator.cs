namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public class CreateImageRestorationJobCommandValidator : AbstractValidator<CreateImageRestorationJobCommand>
{
    public CreateImageRestorationJobCommandValidator()
    {
        RuleFor(v => v.ImageData)
            .NotEmpty().WithMessage("Image data is required.");

        RuleFor(v => v.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(v => v.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(BeAValidImageContentType).WithMessage("Content type must be an image type (e.g., image/jpeg).");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }

    private bool BeAValidImageContentType(string contentType)
    {
        return contentType.StartsWith("image/");
    }
}
