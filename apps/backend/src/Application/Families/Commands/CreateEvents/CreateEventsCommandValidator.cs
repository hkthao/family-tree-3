namespace backend.Application.Events.Commands.CreateEvents;

public class CreateEventsCommandValidator : AbstractValidator<CreateEventsCommand>
{
    public CreateEventsCommandValidator()
    {
        RuleFor(x => x.Events)
            .NotEmpty().WithMessage("Danh sách sự kiện không được để trống.");

        RuleForEach(x => x.Events).SetValidator(new CreateEventDtoValidator());
    }
}
