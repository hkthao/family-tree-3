namespace backend.Application.Members.Commands.SaveAIBiography;

public class SaveAIBiographyCommandValidator : AbstractValidator<SaveAIBiographyCommand>
{
    public SaveAIBiographyCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId cannot be empty.");

        RuleFor(v => v.Style)
            .IsInEnum().WithMessage("Invalid BiographyStyle value.");

        RuleFor(v => v.Content)
            .NotNull().WithMessage("Content cannot be null.")
            .NotEmpty().WithMessage("Content cannot be empty.");

        RuleFor(v => v.Provider)
            .IsInEnum().WithMessage("Invalid AIProviderType value.");

        RuleFor(v => v.UserPrompt)
            .NotNull().WithMessage("UserPrompt cannot be null.")
            .NotEmpty().WithMessage("UserPrompt cannot be empty.");

        RuleFor(v => v.TokensUsed)
            .GreaterThanOrEqualTo(0).WithMessage("TokensUsed cannot be negative.");
    }
}