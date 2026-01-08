namespace backend.Application.Notifications.Commands.SendNotification;

public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(v => v.WorkflowId)
            .NotEmpty().WithMessage("Workflow ID is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.Payload)
            .NotNull().WithMessage("Payload is required.");
    }
}
