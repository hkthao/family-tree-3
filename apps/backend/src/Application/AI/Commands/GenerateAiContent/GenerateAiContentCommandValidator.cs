namespace backend.Application.AI.Commands.GenerateAiContent;

/// <summary>
/// Trình xác thực cho GenerateAiContentCommand.
/// </summary>
public class GenerateAiContentCommandValidator : AbstractValidator<GenerateAiContentCommand>
{
    public GenerateAiContentCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId không được để trống.");

        RuleFor(v => v.ChatInput)
            .NotEmpty().WithMessage("ChatInput không được để trống.")
            .Must(BeWithinWordLimit).WithMessage("ChatInput không được vượt quá 200 từ.");

        RuleFor(v => v.ContentType)
            .NotEmpty().WithMessage("ContentType không được để trống.")
            .Must(BeAValidContentType).WithMessage("ContentType không hợp lệ. Các loại hợp lệ là: family, member, event, chat.");
    }

    private bool BeWithinWordLimit(string chatInput)
    {
        if (string.IsNullOrWhiteSpace(chatInput))
        {
            return true;
        }
        var wordCount = chatInput.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return wordCount <= 200;
    }

    private bool BeAValidContentType(string contentType)
    {
        return contentType.Equals("family", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("member", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("event", StringComparison.OrdinalIgnoreCase) ||
               contentType.Equals("chat", StringComparison.OrdinalIgnoreCase);
    }
}
