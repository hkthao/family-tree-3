using FluentValidation;

namespace backend.Application.UserPushTokens.Commands.RemoveUserPushToken;

public class RemoveUserPushTokenCommandValidator : AbstractValidator<RemoveUserPushTokenCommand>
{
    public RemoveUserPushTokenCommandValidator()
    {
        RuleFor(v => v.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(v => v.ExpoPushToken)
            .NotEmpty().WithMessage("Expo Push Token is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
