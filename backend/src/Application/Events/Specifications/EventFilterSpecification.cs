using backend.Application.Common.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Specifications;

public class EventFilterSpecification : BaseSpecification<Event>
{
    public EventFilterSpecification(
        string? searchTerm,
        EventType? eventType,
        Guid? familyId,
        DateTime? startDate,
        DateTime? endDate,
        string? location,
        Guid? relatedMemberId,
        int skip,
        int take)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            AddCriteria(e => e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm)));
        }

        if (eventType.HasValue)
        {
            AddCriteria(e => e.Type == eventType.Value);
        }

        if (familyId.HasValue)
        {
            AddCriteria(e => e.FamilyId == familyId.Value);
        }

        if (startDate.HasValue)
        {
            AddCriteria(e => e.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            AddCriteria(e => e.EndDate <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(location))
        {
            AddCriteria(e => e.Location != null && e.Location.Contains(location));
        }

        if (relatedMemberId.HasValue)
        {
            AddCriteria(e => e.RelatedMembers.Any(m => m.Id == relatedMemberId.Value));
        }

        ApplyPaging(skip, take);
        AddOrderBy(e => e.StartDate ?? DateTime.MinValue); // Default order by
    }
}