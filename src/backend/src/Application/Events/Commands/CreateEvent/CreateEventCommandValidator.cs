namespace backend.Application.Events.Commands.CreateEvent;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotNull().WithMessage("Name cannot be null.")
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.FamilyId)
            .NotNull().WithMessage("FamilyId cannot be null.")
            .NotEmpty().WithMessage("FamilyId cannot be empty.")
            .Must(id => id != Guid.Empty).WithMessage("FamilyId cannot be empty.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(v => v.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");

        RuleFor(v => v.Color)
            .MaximumLength(20).WithMessage("Color must not exceed 20 characters.");

        RuleFor(v => v.EndDate)
            .GreaterThanOrEqualTo(v => v.StartDate)
            .When(v => v.StartDate.HasValue && v.EndDate.HasValue)
            .WithMessage("EndDate cannot be before StartDate.");
    }
}
