using backend.Application.Common.Models;
using backend.Application.Events.Commands.CreateEvent;

namespace backend.Application.Events.Commands.CreateEvents;

public record CreateEventsCommand(List<CreateEventDto> Events) : IRequest<Result<List<Guid>>>;
