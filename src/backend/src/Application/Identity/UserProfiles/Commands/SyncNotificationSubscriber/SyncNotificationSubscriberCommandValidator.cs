using backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;
using FluentValidation;

namespace backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;

public class SyncNotificationSubscriberCommandValidator : AbstractValidator<SyncNotificationSubscriberCommand>
{
    public SyncNotificationSubscriberCommandValidator()
    {
        RuleFor(v => v.UserProfile)
            .NotNull().WithMessage("User profile cannot be null.");
    }
}
