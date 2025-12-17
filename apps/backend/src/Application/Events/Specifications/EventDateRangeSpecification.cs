using System.Linq.Expressions;
using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventDateRangeSpecification : Specification<Event>
{
    public EventDateRangeSpecification(DateTime? startDate, DateTime? endDate, int? lunarStartDay, int? lunarStartMonth, int? lunarEndDay, int? lunarEndMonth)
    {
        if (startDate.HasValue)
        {
            // For solar events, filter by SolarDate
            // For lunar events, currently we don't have a direct date range on LunarDate, so we will skip for now
            // or we might need a more complex conversion if lunar dates are to be searchable by solar range
            Query.Where(e => (e.CalendarType == Domain.Enums.CalendarType.Solar && e.SolarDate >= startDate.Value) ||
                             (e.CalendarType == Domain.Enums.CalendarType.Lunar &&
                              (!lunarStartDay.HasValue || !lunarStartMonth.HasValue || (e.LunarDate != null &&
                                (e.LunarDate.Day > lunarStartDay.Value ||
                                 (e.LunarDate.Day == lunarStartDay.Value && e.LunarDate.Month >= lunarStartMonth.Value))))));
        }

        if (endDate.HasValue)
        {
            // Similar logic for endDate
            Query.Where(e => (e.CalendarType == Domain.Enums.CalendarType.Solar && e.SolarDate <= endDate.Value) ||
                             (e.CalendarType == Domain.Enums.CalendarType.Lunar &&
                              (!lunarEndDay.HasValue || !lunarEndMonth.HasValue || (e.LunarDate != null &&
                                (e.LunarDate.Day < lunarEndDay.Value ||
                                 (e.LunarDate.Day == lunarEndDay.Value && e.LunarDate.Month <= lunarEndMonth.Value))))));
        }

        // Placeholder for new combined solar OR lunar filtering logic
    }
}
