namespace backend.Application.Notifications.Commands.SaveExpoPushToken;

public class SaveExpoPushTokenCommandValidator : AbstractValidator<SaveExpoPushTokenCommand>
{
    public SaveExpoPushTokenCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
