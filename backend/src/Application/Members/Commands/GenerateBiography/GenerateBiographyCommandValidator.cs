namespace backend.Application.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandValidator : AbstractValidator<GenerateBiographyCommand>
{
    public GenerateBiographyCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId cannot be empty.");

        RuleFor(v => v.Prompt)
            .MaximumLength(1500).WithMessage("Prompt must not exceed 1500 characters.");
    }
}
