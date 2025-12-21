namespace backend.Application.AI.Chat;

/// <summary>
/// Trình xác thực cho <see cref="ChatWithAssistantCommand"/>.
/// </summary>
public class ChatWithAssistantCommandValidator : AbstractValidator<ChatWithAssistantCommand>
{
    public ChatWithAssistantCommandValidator()
    {
        RuleFor(v => v.FamilyId)
            .NotEmpty().WithMessage("FamilyId cannot be empty.");

        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("SessionId cannot be empty.");

        RuleFor(v => v.ChatInput)
            .NotEmpty().WithMessage("ChatInput cannot be empty.")
            .MaximumLength(2000).WithMessage("ChatInput must not exceed 2000 characters.");
    }
}
