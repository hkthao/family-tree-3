using FluentValidation;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Commands.RecordActivity;

public class RecordActivityCommandValidator : AbstractValidator<RecordActivityCommand>
{
    public RecordActivityCommandValidator()
    {
        RuleFor(v => v.UserProfileId)
            .NotEmpty().WithMessage("UserProfileId cannot be empty.");

        RuleFor(v => v.ActionType)
            .IsInEnum().WithMessage("Invalid ActionType value.");

        RuleFor(v => v.TargetType)
            .IsInEnum().WithMessage("Invalid TargetType value.");

        RuleFor(v => v.ActivitySummary)
            .NotNull().WithMessage("ActivitySummary cannot be null.")
            .NotEmpty().WithMessage("ActivitySummary cannot be empty.");
    }
}
