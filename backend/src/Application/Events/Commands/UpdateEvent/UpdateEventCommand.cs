using backend.Application.Events.Commands.Inputs;
using MediatR;

namespace backend.Application.Events.Commands.UpdateEvent;

public record UpdateEventCommand : EventInput, IRequest
{
    public Guid Id { get; init; }
}