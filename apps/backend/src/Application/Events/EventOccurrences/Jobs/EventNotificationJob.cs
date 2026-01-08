using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.EventOccurrences.Jobs;

public class EventNotificationJob : IEventNotificationJob
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IDateTime _dateTime;
    private readonly ILogger<EventNotificationJob> _logger;

    public EventNotificationJob(IApplicationDbContext context, INotificationService notificationService, IDateTime dateTime, ILogger<EventNotificationJob> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("EventNotificationJob started at {Timestamp}", _dateTime.Now);

        var today = _dateTime.Now.Date;
        var threeDaysLater = today.AddDays(3);

        // Lấy tất cả EventOccurrences trong 3 ngày tới
        var upcomingOccurrences = await _context.EventOccurrences
            .Where(eo => eo.OccurrenceDate.Date >= today && eo.OccurrenceDate.Date <= threeDaysLater)
            .OrderBy(eo => eo.OccurrenceDate)
            .ToListAsync(cancellationToken);

        var eventIds = upcomingOccurrences.Select(eo => eo.EventId).Distinct().ToList();

        // Lấy thông tin Event cho các EventId này
        var events = await _context.Events
            .Where(e => eventIds.Contains(e.Id))
            .Include(e => e.Family) // Nếu cần thông tin family cho thông báo
            .ToListAsync(cancellationToken);

        foreach (var occurrence in upcomingOccurrences)
        {
            var @event = events.FirstOrDefault(e => e.Id == occurrence.EventId);
            if (@event == null)
            {
                _logger.LogWarning("Event with ID {EventId} not found for occurrence date {OccurrenceDate}", occurrence.EventId, occurrence.OccurrenceDate);
                continue;
            }

            // Kiểm tra xem thông báo đã được gửi cho sự kiện này vào ngày này chưa
            var existingDelivery = await _context.NotificationDeliveries
                .FirstOrDefaultAsync(nd => nd.EventId == occurrence.EventId &&
                                           nd.DeliveryDate.Date == occurrence.OccurrenceDate.Date &&
                                           nd.DeliveryMethod == "Push Notification", // Giả định là Push Notification
                                           cancellationToken);

            // Xác định bản ghi delivery để làm việc
            // Nếu đã gửi thành công trước đó, bỏ qua
            if (existingDelivery != null && existingDelivery.IsSent)
            {
                _logger.LogInformation("Notification already sent for Event {EventName} ({EventId}) on {OccurrenceDate}", @event.Name, @event.Id, occurrence.OccurrenceDate.Date);
                continue;
            }

            // Nếu không có bản ghi delivery hiện có hoặc nó chưa được gửi
            if (existingDelivery == null)
            {
                existingDelivery = NotificationDelivery.Create(occurrence.EventId, null, occurrence.OccurrenceDate.Date, "Push Notification");
                _context.NotificationDeliveries.Add(existingDelivery); // Thêm bản ghi mới nếu không tồn tại
            }

            // Tạo payload thông báo
            var notificationPayload = new
            {
                Title = $"Sự kiện sắp diễn ra: {@event.Name}",
                Body = $"Sự kiện '{@event.Name}' của gia đình {@event.Family?.Name ?? "không rõ"} sẽ diễn ra vào ngày {occurrence.OccurrenceDate.ToShortDateString()}.",
                EventId = @event.Id,
                EventType = @event.Type.ToString(),
                FamilyId = @event.FamilyId
            };

            // Gửi thông báo
            try
            {
                var sendResult = await _notificationService.SendNotificationAsync("event-upcoming", "all", notificationPayload, cancellationToken);

                if (sendResult.IsSuccess)
                {
                    existingDelivery.MarkAsSent(); // This is supposed to set IsSent = true
                    _logger.LogInformation("Notification sent successfully...");
                }
                else
                {
                    existingDelivery.IncrementAttempts();
                    _logger.LogError("Failed to send notification for Event {EventName} ({EventId}) on {OccurrenceDate}: {Error}", @event.Name, @event.Id, occurrence.OccurrenceDate.Date, sendResult.Error);
                }
            }
            catch (Exception ex)
            {
                existingDelivery.IncrementAttempts();
                _logger.LogError(ex, "Exception while sending notification for Event {EventName} ({EventId}) on {OccurrenceDate}", @event.Name, @event.Id, occurrence.OccurrenceDate.Date);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("EventNotificationJob finished at {Timestamp}", _dateTime.Now);
    }
}
