using FluentValidation;

namespace backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;

public class DeleteNotificationTemplateCommandValidator : AbstractValidator<DeleteNotificationTemplateCommand>
{
    public DeleteNotificationTemplateCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID không được để trống.");
    }
}
