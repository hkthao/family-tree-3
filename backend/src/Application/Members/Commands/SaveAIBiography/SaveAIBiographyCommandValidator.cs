namespace backend.Application.Members.Commands.SaveAIBiography
{
    public class SaveAIBiographyCommandValidator : AbstractValidator<SaveAIBiographyCommand>
    {
        public SaveAIBiographyCommandValidator()
        {
            RuleFor(v => v.MemberId)
                .NotEmpty().WithMessage("MemberId is required.");

            RuleFor(v => v.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(4000).WithMessage("Content must not exceed 4000 characters."); // Assuming a reasonable max length

            RuleFor(v => v.UserPrompt)
                .MaximumLength(1000).WithMessage("UserPrompt must not exceed 1000 characters."); // Assuming a reasonable max length

            RuleFor(v => v.TokensUsed)
                .GreaterThanOrEqualTo(0).WithMessage("TokensUsed must be a non-negative number.");
        }
    }
}
