using backend.Domain.Enums;
using backend.Application.Events.Commands.Inputs; // Add this

namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventDtoValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên sự kiện không được để trống.")
            .MaximumLength(200).WithMessage("Tên sự kiện không được vượt quá 200 ký tự.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã sự kiện không được để trống.")
            .MaximumLength(50).WithMessage("Mã sự kiện không được vượt quá 50 ký tự.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("ID gia đình không được để trống.")
            .Must(id => id != Guid.Empty).WithMessage("ID gia đình không được để trống.");

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
                .ChildRules(ld => {
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
