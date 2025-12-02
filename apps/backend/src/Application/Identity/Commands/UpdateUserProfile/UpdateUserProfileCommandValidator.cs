using backend.Application.Common.Constants; // New using
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

        RuleFor(v => v.AvatarBase64)
            .MaximumLength(ImageConstants.MaxAvatarBase64Length).When(v => !string.IsNullOrEmpty(v.AvatarBase64))
            .WithMessage($"AvatarBase64 must not exceed {ImageConstants.MaxAvatarBase64Length} characters.");
    }
}
