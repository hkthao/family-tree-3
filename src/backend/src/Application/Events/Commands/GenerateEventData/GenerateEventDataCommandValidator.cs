namespace backend.Application.Events.Commands.GenerateEventData;

public class GenerateEventDataCommandValidator : AbstractValidator<GenerateEventDataCommand>
{
    public GenerateEventDataCommandValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt không được để trống.");
    }
}
