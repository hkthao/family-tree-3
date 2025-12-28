using backend.Application.Common.Models;
using backend.Application.Events.Queries; // Changed

namespace backend.Application.Events.Commands.ImportEvents;

public record ImportEventsCommand(Guid FamilyId, List<EventDto> Events) : IRequest<Result<Unit>>;
