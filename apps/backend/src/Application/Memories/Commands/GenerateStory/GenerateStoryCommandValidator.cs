using FluentValidation;

namespace backend.Application.Memories.Commands.GenerateStory;

public class GenerateStoryCommandValidator : AbstractValidator<GenerateStoryCommand>
{
    public GenerateStoryCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("Member ID is required.");

        RuleFor(v => v.RawText)
            .NotEmpty().When(v => !v.PhotoAnalysisId.HasValue).WithMessage("Raw text is required if no photo analysis is provided.")
            .MinimumLength(10).When(v => !v.PhotoAnalysisId.HasValue).WithMessage("Raw text must be at least 10 characters long if no photo analysis is provided.")
            .MaximumLength(2000).WithMessage("Raw text must not exceed 2000 characters."); // Arbitrary max length

        RuleFor(v => v.Style)
            .NotEmpty().WithMessage("Style is required.")
            .Must(BeAValidStyle).WithMessage("Invalid style provided.");

        RuleFor(v => v.MaxWords)
            .InclusiveBetween(100, 1000).WithMessage("Max words must be between 100 and 1000.");
    }

    private bool BeAValidStyle(string style)
    {
        // Define your valid styles here
        var validStyles = new[] { "nostalgic", "warm", "formal", "folk" };
        return validStyles.Contains(style.ToLower());
    }
}
