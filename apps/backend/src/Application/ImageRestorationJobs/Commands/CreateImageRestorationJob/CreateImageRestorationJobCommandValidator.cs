namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public class CreateImageRestorationJobCommandValidator : AbstractValidator<CreateImageRestorationJobCommand>
{
    public CreateImageRestorationJobCommandValidator()
    {
        RuleFor(v => v.OriginalImageUrl)
            .NotEmpty().WithMessage("Original Image URL is required.")
            .Matches(@"^(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$").WithMessage("Original Image URL must be a valid URL.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}
