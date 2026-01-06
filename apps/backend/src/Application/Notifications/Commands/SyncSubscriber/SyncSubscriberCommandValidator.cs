namespace backend.Application.Notifications.Commands.SyncSubscriber;

public class SyncSubscriberCommandValidator : AbstractValidator<SyncSubscriberCommand>
{
    public SyncSubscriberCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
