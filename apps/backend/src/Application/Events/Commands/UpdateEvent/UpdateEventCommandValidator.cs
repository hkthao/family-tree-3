using backend.Domain.Enums; // Add this

namespace backend.Application.Events.Commands.UpdateEvent;

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id cannot be empty.");

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

        RuleFor(v => v.Color)
            .MaximumLength(20).WithMessage("Color must not exceed 20 characters.");

        RuleFor(v => v.CalendarType)
            .IsInEnum().WithMessage("Invalid CalendarType.");

        RuleFor(v => v.RepeatRule)
            .IsInEnum().WithMessage("Invalid RepeatRule.");

        // Conditional validation for Solar and Lunar dates
        When(v => v.CalendarType == CalendarType.Solar, () =>
        {
            RuleFor(v => v.SolarDate)
                .NotNull().WithMessage("Solar event must have a SolarDate.");
            RuleFor(v => v.LunarDate)
                .Null().WithMessage("Solar event cannot have a LunarDate.");
        });

        When(v => v.CalendarType == CalendarType.Lunar, () =>
        {
            RuleFor(v => v.LunarDate)
                .NotNull().WithMessage("Lunar event must have a LunarDate.")
                .ChildRules(ld =>
                {
                    ld.RuleFor(x => x!.Day)
                        .InclusiveBetween(1, 30).WithMessage("Lunar day must be between 1 and 30.");
                    ld.RuleFor(x => x!.Month)
                        .InclusiveBetween(1, 12).WithMessage("Lunar month must be between 1 and 12.");
                });
            RuleFor(v => v.SolarDate)
                .Null().WithMessage("Lunar event cannot have a SolarDate.");
        });
    }
}
