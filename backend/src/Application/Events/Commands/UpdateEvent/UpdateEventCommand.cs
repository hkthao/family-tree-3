using backend.Application.Common.Models; // Added
using backend.Application.Events.Commands.Inputs;

namespace backend.Application.Events.Commands.UpdateEvent
{
    public record UpdateEventCommand : EventInput, IRequest<Result<bool>>
    {
        public Guid Id { get; init; }
    }
}
