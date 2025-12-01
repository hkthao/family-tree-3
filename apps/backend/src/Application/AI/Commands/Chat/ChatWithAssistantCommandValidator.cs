namespace backend.Application.AI.Chat;

/// <summary>
/// Trình xác thực cho <see cref="ChatWithAssistantCommand"/>.
/// </summary>
public class ChatWithAssistantCommandValidator : AbstractValidator<ChatWithAssistantCommand>
{
    public ChatWithAssistantCommandValidator()
    {
        RuleFor(v => v.SessionId)
            .NotEmpty().WithMessage("SessionId cannot be empty.");

        RuleFor(v => v.Message)
            .NotEmpty().WithMessage("Message cannot be empty.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");
    }
}
