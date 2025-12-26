using backend.Application.AI.Commands.DetermineChatContext; // Update using directive for the command
using FluentValidation;

namespace backend.Application.AI.Commands.DetermineChatContext; // New namespace

/// <summary>
/// Trình xác thực cho DetermineChatContextCommand.
/// </summary>
public class DetermineChatContextCommandValidator : AbstractValidator<DetermineChatContextCommand>
{
    public DetermineChatContextCommandValidator()
    {
        RuleFor(x => x.ChatMessage)
            .NotEmpty().WithMessage("Tin nhắn chat không được để trống.");

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID không được để trống.");
    }
}
