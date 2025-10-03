using backend.Application.Events.Commands.Inputs;

namespace backend.Application.Events.Commands.CreateEvent;

public record CreateEventCommand : EventInput, IRequest<Guid>
{
}