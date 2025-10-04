using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Specifications;

public class EventFilterSpecification : Specification<Event>
{
    public EventFilterSpecification(string? searchTerm, EventType? eventType, Guid? familyId, DateTime? startDate, DateTime? endDate, string? location, Guid? relatedMemberId, int skip, int take)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Query.Where(e => e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm)));
        }

        if (eventType.HasValue)
        {
            Query.Where(e => e.Type == eventType.Value);
        }

        if (startDate.HasValue)
        {
            Query.Where(e => e.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            Query.Where(e => e.EndDate <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(location))
        {
            Query.Where(e => e.Location != null && e.Location.Contains(location));
        }

        if (relatedMemberId.HasValue)
        {
            // This assumes a relationship between Event and Member that might need to be defined.
            // For now, I'll assume a simple property. This might need adjustment.
            // Query.Where(e => e.RelatedMemberId == relatedMemberId.Value);
        }

        Query.Skip(skip).Take(take);
    }
}