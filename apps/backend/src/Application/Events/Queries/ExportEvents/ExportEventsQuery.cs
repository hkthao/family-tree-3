using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.ExportEvents;

public record ExportEventsQuery(Guid FamilyId) : IRequest<Result<string>>;
