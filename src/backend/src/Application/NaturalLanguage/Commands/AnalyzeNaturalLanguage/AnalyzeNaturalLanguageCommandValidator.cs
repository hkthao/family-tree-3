namespace backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;

/// <summary>
/// Validator cho AnalyzeNaturalLanguageCommand.
/// </summary>
public class AnalyzeNaturalLanguageCommandValidator : AbstractValidator<AnalyzeNaturalLanguageCommand>
{
    public AnalyzeNaturalLanguageCommandValidator()
    {
        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Nội dung không được để trống.")
            .MaximumLength(2000).WithMessage("Nội dung không được vượt quá 2000 ký tự.");

        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("SessionId không được để trống.");
    }
}
