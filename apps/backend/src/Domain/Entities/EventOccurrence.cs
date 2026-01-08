using System;

namespace backend.Domain.Entities;

public class EventOccurrence : BaseAuditableEntity
{
    public Guid EventId { get; set; }
    public int Year { get; set; }
    public DateTime OccurrenceDate { get; set; }

    // Navigation property
    public Event Event { get; set; } = null!;

    private EventOccurrence() { }

    public static EventOccurrence Create(Guid eventId, int year, DateTime occurrenceDate)
    {
        return new EventOccurrence
        {
            EventId = eventId,
            Year = year,
            OccurrenceDate = occurrenceDate
        };
    }
}
