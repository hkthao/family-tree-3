using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByMemberIdSpecification : Specification<Event>
{
    public EventByMemberIdSpecification(Guid? memberId)
    {
        if (memberId.HasValue)
        {
            Query.Where(e => e.EventMembers.Any(em => em.MemberId == memberId.Value));
        }
    }
}
