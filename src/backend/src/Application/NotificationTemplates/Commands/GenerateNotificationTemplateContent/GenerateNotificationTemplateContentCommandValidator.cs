using FluentValidation;

namespace backend.Application.NotificationTemplates.Commands.GenerateNotificationTemplateContent;

public class GenerateNotificationTemplateContentCommandValidator : AbstractValidator<GenerateNotificationTemplateContentCommand>
{
    public GenerateNotificationTemplateContentCommandValidator()
    {
        RuleFor(v => v.Prompt)
            .NotEmpty().WithMessage("Lời nhắc không được để trống.")
            .MinimumLength(10).WithMessage("Lời nhắc phải có ít nhất 10 ký tự.")
            .MaximumLength(1000).WithMessage("Lời nhắc không được vượt quá 1000 ký tự.");
    }
}
