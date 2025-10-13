namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandValidator : AbstractValidator<UpdateFamilyCommand>
{
    public UpdateFamilyCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id cannot be empty.");

        RuleFor(v => v.Name)
            .NotNull().WithMessage("Name cannot be null.")
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(v => v.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(v => v.AvatarUrl)
            .MaximumLength(2048).WithMessage("AvatarUrl must not exceed 2048 characters.")
            .Matches(@"^(https?://)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*/?$").When(v => !string.IsNullOrEmpty(v.AvatarUrl))
            .WithMessage("AvatarUrl must be a valid URL.");

        RuleFor(v => v.Visibility)
            .NotNull().WithMessage("Visibility cannot be null.")
            .NotEmpty().WithMessage("Visibility cannot be empty.")
            .Must(BeAValidVisibility).WithMessage("Visibility must be 'Public' or 'Private'.");
    }

    private bool BeAValidVisibility(string visibility)
    {
        return visibility == "Public" || visibility == "Private";
    }
}