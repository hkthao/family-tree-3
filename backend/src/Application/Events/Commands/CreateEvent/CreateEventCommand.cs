using backend.Application.Events.Commands.Inputs;
using MediatR;

namespace backend.Application.Events.Commands.CreateEvent;

public record CreateEventCommand : EventInput, IRequest<Guid>
{
}