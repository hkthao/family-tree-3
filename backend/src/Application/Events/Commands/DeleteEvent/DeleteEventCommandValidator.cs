using FluentValidation;

namespace backend.Application.Events.Commands.DeleteEvent;

public class DeleteEventCommandValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id cannot be empty.");
    }
}