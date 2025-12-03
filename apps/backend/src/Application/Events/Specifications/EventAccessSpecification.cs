using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventAccessSpecification : Specification<Event>
{
    public EventAccessSpecification(bool isAdmin, Guid currentUserId)
    {
        Query.Include(e => e.Family!);
        Query.Include(e => e.Family!.FamilyUsers!);
        if (!isAdmin)
            Query.Where(e => e.Family != null && e.Family.FamilyUsers!.Any(fu => fu.UserId == currentUserId));
    }
}
