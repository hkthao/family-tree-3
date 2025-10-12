using backend.Application.Common.Models; // Added

namespace backend.Application.Events.Commands.DeleteEvent;

public record DeleteEventCommand(Guid Id) : IRequest<Result<bool>>;
