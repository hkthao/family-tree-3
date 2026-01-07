using FluentValidation;

namespace backend.Application.UserPushTokens.Commands.SyncCurrentUserPushToken;

public class SyncCurrentUserPushTokenCommandValidator : AbstractValidator<SyncCurrentUserPushTokenCommand>
{
    public SyncCurrentUserPushTokenCommandValidator()
    {
        RuleFor(v => v.ExpoPushToken)
            .NotEmpty().WithMessage("ExpoPushToken không được để trống.");

        RuleFor(v => v.Platform)
            .NotEmpty().WithMessage("Platform không được để trống.");

        RuleFor(v => v.DeviceId)
            .NotEmpty().WithMessage("DeviceId không được để trống.");
    }
}
