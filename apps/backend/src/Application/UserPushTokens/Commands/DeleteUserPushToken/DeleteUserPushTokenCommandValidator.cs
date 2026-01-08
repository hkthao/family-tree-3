namespace backend.Application.UserPushTokens.Commands.DeleteUserPushToken;

public class DeleteUserPushTokenCommandValidator : AbstractValidator<DeleteUserPushTokenCommand>
{
    public DeleteUserPushTokenCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id không được để trống.");
    }
}
