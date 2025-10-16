using FluentValidation;
using backend.Application.Events.Queries;
using backend.Domain.Enums;

namespace backend.Application.Events.Commands.GenerateEventData;

public class AIEventDtoValidator : AbstractValidator<AIEventDto>
{
    public AIEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(100).WithMessage("Event name cannot exceed 100 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Event type is required.")
            .Must(BeAValidEventType).WithMessage("Invalid event type.");

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after or equal to start date.");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.FamilyName)
            .MaximumLength(100).WithMessage("Family name cannot exceed 100 characters.");
    }

    private bool BeAValidEventType(string type)
    {
        return Enum.TryParse<EventType>(type, true, out _);
    }
}
