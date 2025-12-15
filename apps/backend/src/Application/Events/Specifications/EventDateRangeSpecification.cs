using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventDateRangeSpecification : Specification<Event>
{
    public EventDateRangeSpecification(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue)
        {
            // For solar events, filter by SolarDate
            // For lunar events, currently we don't have a direct date range on LunarDate, so we will skip for now
            // or we might need a more complex conversion if lunar dates are to be searchable by solar range
            Query.Where(e => e.CalendarType == Domain.Enums.CalendarType.Solar && e.SolarDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Similar logic for endDate
            Query.Where(e => e.CalendarType == Domain.Enums.CalendarType.Solar && e.SolarDate <= endDate.Value);
        }
    }
}
