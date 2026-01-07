namespace backend.Application.UserPushTokens.Commands.RemoveCurrentUserPushToken;

public class RemoveCurrentUserPushTokenCommandValidator : AbstractValidator<RemoveCurrentUserPushTokenCommand>
{
    public RemoveCurrentUserPushTokenCommandValidator()
    {
        RuleFor(v => v.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(v => v.ExpoPushToken)
            .NotEmpty().WithMessage("Expo Push Token is required.");
    }
}
