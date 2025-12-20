using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetAllEventsByFamilyId;

public record GetAllEventsByFamilyIdQuery : IRequest<Result<List<EventDto>>>
{
    public Guid FamilyId { get; init; }
}
