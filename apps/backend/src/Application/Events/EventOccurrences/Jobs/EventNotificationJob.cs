using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Domain.Entities;
using backend.Domain.Enums; // NEW: For Gender enum
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.EventOccurrences.Jobs;

public class EventNotificationJob(IApplicationDbContext context, INotificationService notificationService, IDateTime dateTime, ILogger<EventNotificationJob> logger) : IEventNotificationJob
{
    private readonly IApplicationDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IDateTime _dateTime = dateTime;
    private readonly ILogger<EventNotificationJob> _logger = logger;

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

        _logger.LogInformation("today: {today}", today);
        _logger.LogInformation("threeDaysLater: {threeDaysLater}", threeDaysLater);
        _logger.LogInformation("upcomingOccurrences: {Count} items", upcomingOccurrences.Count);

        var eventIds = upcomingOccurrences.Select(eo => eo.EventId).Distinct().ToList();

        // Lấy thông tin Event cho các EventId này, eager loading EventMembers và Member cho thông tin giới tính
        var events = await _context.Events
            .Where(e => eventIds.Contains(e.Id))
            .Include(e => e.Family)
            .Include(e => e.EventMembers).ThenInclude(em => em.Member) // NEW: Eager load EventMembers and Member
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
                                           //nd.DeliveryDate.Date == occurrence.OccurrenceDate.Date &&
                                           nd.DeliveryMethod == "Push Notification",
                                           cancellationToken);

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
                _context.NotificationDeliveries.Add(existingDelivery);
            }

            // NEW: Determine titles based on gender
            string memberHonorific = "Sự kiện sắp tới"; // Default title
            string memberName = "";

            var primaryEventMember = @event.EventMembers?.FirstOrDefault(); // Get the first related member
            if (primaryEventMember?.Member != null)
            {
                memberName = primaryEventMember.Member.FirstName ?? primaryEventMember.Member.FullName ?? "";
                if (Enum.TryParse<Gender>(primaryEventMember.Member.Gender, true, out var memberGender) && memberGender == Gender.Male)
                {
                    memberHonorific = $"Ông {memberName}";
                }
                else if (Enum.TryParse<Gender>(primaryEventMember.Member.Gender, true, out memberGender) && memberGender == Gender.Female)
                {
                    memberHonorific = $"Bà {memberName}";
                }
            }
            else
            {
                memberHonorific = $"Sự kiện sắp tới: {@event.Name}"; // Fallback if no specific member for honorific
            }

            // NEW: Retrieve recipient user IDs
            var familyUserIds = await _context.FamilyUsers
                .Where(fu => fu.FamilyId == @event.FamilyId)
                .Select(fu => fu.UserId.ToString())
                .ToListAsync(cancellationToken);

            var familyFollowerIds = await _context.FamilyFollows
                .Where(ff => ff.FamilyId == @event.FamilyId)
                .Select(ff => ff.UserId.ToString())
                .ToListAsync(cancellationToken);

            var recipientUserIds = familyUserIds.Union(familyFollowerIds).ToList();

            if (!recipientUserIds.Any())
            {
                _logger.LogInformation("No recipients found for Event {EventName} ({EventId}) on {OccurrenceDate}. Skipping notification.", @event.Name, @event.Id, occurrence.OccurrenceDate.Date);
                continue;
            }

            // Tạo payload thông báo theo scheme từ @task.md
            var notificationPayload = new
            {
                event_id = @event.Id.ToString(),
                event_name = @event.Name,
                event_type = @event.Type.ToString(),
                familyId = @event.FamilyId.HasValue ? @event.FamilyId.Value.ToString() : string.Empty,
                member_name = memberName, // Use determined member name
                lunar_date = @event.LunarDate != null ? @event.LunarDate.ToString() : string.Empty,
                event_date = occurrence.OccurrenceDate.ToString("dd/MM"), // Format as dd/MM
                titles = memberHonorific // Use determined honorific
            };

            // Gửi thông báo
            try
            {
                var sendResult = await _notificationService.SendNotificationAsync(
                    "event-upcoming",
                    recipientUserIds, // NEW: Pass list of recipient user IDs
                    notificationPayload,
                    cancellationToken
                );

                if (sendResult.IsSuccess)
                {
                    existingDelivery.MarkAsSent();
                    _logger.LogInformation("Notification sent successfully to {RecipientCount} recipients for Event {EventName} ({EventId}) on {OccurrenceDate}", recipientUserIds.Count, @event.Name, @event.Id, occurrence.OccurrenceDate.Date);
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
