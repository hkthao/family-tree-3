using backend.Domain.Common;

namespace backend.Domain.Entities;

public class NotificationDelivery : BaseAuditableEntity
{
    public Guid EventId { get; set; }
    public Guid? MemberId { get; set; } // Null if not member-specific
    public DateTime DeliveryDate { get; set; }
    public string DeliveryMethod { get; set; } = null!; // e.g., "Push Notification", "Email"
    public bool IsSent { get; set; }
    public int SentAttempts { get; set; }

    public Event Event { get; set; } = null!; // Navigation property

    private NotificationDelivery() { }

    public static NotificationDelivery Create(Guid eventId, Guid? memberId, DateTime deliveryDate, string deliveryMethod)
    {
        return new NotificationDelivery
        {
            EventId = eventId,
            MemberId = memberId,
            DeliveryDate = deliveryDate,
            DeliveryMethod = deliveryMethod,
            IsSent = false,
            SentAttempts = 0
        };
    }

    public void MarkAsSent()
    {
        IsSent = true;
        SentAttempts++;
    }

    public void IncrementAttempts()
    {
        SentAttempts++;
    }
}
