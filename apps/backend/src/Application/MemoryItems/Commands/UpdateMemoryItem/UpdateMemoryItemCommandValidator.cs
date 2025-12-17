namespace backend.Application.MemoryItems.Commands.UpdateMemoryItem;

public class UpdateMemoryItemCommandValidator : AbstractValidator<UpdateMemoryItemCommand>
{
    public UpdateMemoryItemCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Memory Item ID is required.");

        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.HappenedAt)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("HappenedAt cannot be in the future.");

        RuleFor(v => v.EmotionalTag)
            .IsInEnum().WithMessage("Invalid EmotionalTag value.");

        RuleForEach(v => v.Media).SetValidator(new UpdateMemoryMediaCommandDtoValidator());
    }
}

public class UpdateMemoryMediaCommandDtoValidator : AbstractValidator<UpdateMemoryMediaCommandDto>
{
    public UpdateMemoryMediaCommandDtoValidator()
    {
        RuleFor(v => v.Id)
            .Must(id => !id.HasValue || id.Value != Guid.Empty).WithMessage("Media ID must be a valid GUID if provided.");

        RuleFor(v => v.Url)
            .NotEmpty().WithMessage("Media URL is required.")
            .MaximumLength(1000).WithMessage("Media URL must not exceed 1000 characters.")
            .Must(BeAValidUrl).WithMessage("Media URL must be a valid URL.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}