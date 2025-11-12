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

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Ngày kết thúc không được trước ngày bắt đầu.");
    }
}
