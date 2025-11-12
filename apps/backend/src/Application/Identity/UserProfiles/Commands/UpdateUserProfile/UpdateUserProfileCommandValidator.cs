namespace backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotNull().WithMessage("Id cannot be null.")
            .NotEmpty().WithMessage("Id cannot be empty.");

        RuleFor(v => v.Name)
            .NotNull().WithMessage("Name cannot be null.")
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(v => v.Email)
            .NotNull().WithMessage("Email cannot be null.")
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(v => v.Avatar)
            .MaximumLength(2048).WithMessage("Avatar URL must not exceed 2048 characters.")
            .Must(BeAValidUrl).When(v => !string.IsNullOrEmpty(v.Avatar)).WithMessage("Avatar URL must be a valid URL.");
    }

    private bool BeAValidUrl(string? url)
    {
        return string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
