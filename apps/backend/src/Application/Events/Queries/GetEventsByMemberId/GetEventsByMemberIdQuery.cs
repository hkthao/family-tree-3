using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetEventsByMemberId;

public record GetEventsByMemberIdQuery : IRequest<Result<List<EventDto>>>
{
    public Guid MemberId { get; init; }
}
