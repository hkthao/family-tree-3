using backend.Application.Common.Models;

namespace backend.Application.Events.Commands.CreateEvents;

public record CreateEventsCommand(List<CreateEventDto> Events) : IRequest<Result<List<Guid>>>;
