namespace backend.Application.Identity.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.Name)
            .MaximumLength(250).WithMessage("Name must not exceed 250 characters.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(v => v.Picture)
            .Must(BeAValidUrl).WithMessage("Picture URL must be a valid URL.");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
