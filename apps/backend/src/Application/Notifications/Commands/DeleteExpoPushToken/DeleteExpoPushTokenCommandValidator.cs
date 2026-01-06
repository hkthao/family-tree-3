namespace backend.Application.Notifications.Commands.DeleteExpoPushToken;

public class DeleteExpoPushTokenCommandValidator : AbstractValidator<DeleteExpoPushTokenCommand>
{
    public DeleteExpoPushTokenCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.ExpoPushToken)
            .NotEmpty().WithMessage("Expo Push Token is required.");
    }
}
