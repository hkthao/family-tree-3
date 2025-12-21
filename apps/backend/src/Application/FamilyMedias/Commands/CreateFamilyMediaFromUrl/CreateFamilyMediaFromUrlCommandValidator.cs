using FluentValidation;

namespace backend.Application.FamilyMedias.Commands.CreateFamilyMediaFromUrl;

public class CreateFamilyMediaFromUrlCommandValidator : AbstractValidator<CreateFamilyMediaFromUrlCommand>
{
    public CreateFamilyMediaFromUrlCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(v => v.Url)
            .NotEmpty().WithMessage("URL is required."); // Prioritize this message

        RuleFor(v => v.Url)
            .MaximumLength(2000).WithMessage("URL must not exceed 2000 characters.")
            .Must(BeAValidUrl).WithMessage("URL must be a valid absolute URL.")
            .When(v => !string.IsNullOrEmpty(v.Url)); // Apply these rules only if URL is not empty

        RuleFor(v => v.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(250).WithMessage("File name must not exceed 250 characters.");

        RuleFor(v => v.MediaType)
            .IsInEnum().WithMessage("Invalid media type.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
