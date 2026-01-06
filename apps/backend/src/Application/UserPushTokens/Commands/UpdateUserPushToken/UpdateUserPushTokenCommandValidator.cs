namespace backend.Application.UserPushTokens.Commands.UpdateUserPushToken;

public class UpdateUserPushTokenCommandValidator : AbstractValidator<UpdateUserPushTokenCommand>
{
    public UpdateUserPushTokenCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id không được để trống.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId không được để trống.");

        RuleFor(v => v.ExpoPushToken)
            .NotEmpty().WithMessage("ExpoPushToken không được để trống.")
            .MaximumLength(500).WithMessage("ExpoPushToken không được vượt quá 500 ký tự.");

        RuleFor(v => v.Platform)
            .NotEmpty().WithMessage("Platform không được để trống.")
            .MaximumLength(50).WithMessage("Platform không được vượt quá 50 ký tự.");

        RuleFor(v => v.DeviceId)
            .NotEmpty().WithMessage("DeviceId không được để trống.")
            .MaximumLength(200).WithMessage("DeviceId không được vượt quá 200 ký tự.");
    }
}
