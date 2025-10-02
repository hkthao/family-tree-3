namespace backend.Application.Events.Commands.DeleteEvent;

public record DeleteEventCommand(Guid Id) : IRequest;
