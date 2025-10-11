namespace backend.Application.Events.Commands.CreateEvent
{
    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200)
                .NotEmpty();

            RuleFor(v => v.Description)
                .MaximumLength(1000);

            RuleFor(v => v.StartDate)
                .NotEmpty();

            RuleFor(v => v.Location)
                .MaximumLength(200);

            RuleFor(v => v.Color)
                .MaximumLength(20);
        }
    }
}
