using backend.Application.Common.Models; // Added
using backend.Application.Events.Commands.Inputs;

namespace backend.Application.Events.Commands.CreateEvent
{
    public record CreateEventCommand : EventInput, IRequest<Result<Guid>>
    {
    }
}
