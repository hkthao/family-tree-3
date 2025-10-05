using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Specifications;

public class EventFilterSpecification : Specification<Event>
{
    public EventFilterSpecification(string? searchTerm, DateTime? startDate, DateTime? endDate, string? type, Guid? familyId, Guid? memberId)
    {
        Query.Where(e =>
            (searchTerm == null || e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm))))
            .Where(e =>
            (startDate == null || e.StartDate >= startDate.Value))
            .Where(e =>
            (endDate == null || e.StartDate <= endDate.Value));

        if (!string.IsNullOrEmpty(type))
        {
            if (Enum.TryParse<EventType>(type, true, out var eventType))
            {
                Query.Where(e => e.Type == eventType);
            }
        }

        if (familyId.HasValue)
        {
            Query.Where(e => e.FamilyId == familyId.Value);
        }

        if (memberId.HasValue)
        {
            Query.Where(e => e.EventMembers.Any(em => em.MemberId == memberId.Value));
        }
    }
}
