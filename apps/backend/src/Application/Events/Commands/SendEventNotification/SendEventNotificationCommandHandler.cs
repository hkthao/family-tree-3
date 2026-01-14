using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.SendEventNotification;

public class SendEventNotificationCommandHandler : IRequestHandler<SendEventNotificationCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IDateTime _dateTime;
    private readonly ILogger<SendEventNotificationCommandHandler> _logger;
    private readonly IAuthorizationService _authorizationService;

    public SendEventNotificationCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService,
        IDateTime dateTime,
        ILogger<SendEventNotificationCommandHandler> logger,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _notificationService = notificationService;
        _dateTime = dateTime;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public async Task<Result<string>> Handle(SendEventNotificationCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.IsAdmin())
        {
            _logger.LogWarning("Unauthorized attempt to send event notification directly by non-admin user for EventId {EventId}.", request.EventId);
            return Result<string>.Failure("Access Denied: Only administrators can send event notifications directly.", ErrorSources.Forbidden);
        }

        _logger.LogInformation($"Handling SendEventNotificationCommand for EventId {request.EventId}.");

        var @event = await _context.Events
            .Where(e => e.Id == request.EventId && !e.IsDeleted)
            .Include(e => e.Family)
            .Include(e => e.EventMembers).ThenInclude(em => em.Member)
            .FirstOrDefaultAsync(cancellationToken);

        if (@event == null)
        {
            _logger.LogWarning("Event with ID {EventId} not found.", request.EventId);
            return Result<string>.Failure(string.Format(ErrorMessages.EventNotFound, request.EventId), ErrorSources.NotFound);
        }

        // For manual notification, we take today's occurrence date, or the event's solar date if no occurrence
        DateTime notificationDate;
        var currentOccurrence = await _context.EventOccurrences
            .Where(eo => eo.EventId == request.EventId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentOccurrence != null)
        {
            notificationDate = currentOccurrence.OccurrenceDate;
        }
        else if (@event.SolarDate.HasValue)
        {
            notificationDate = @event.SolarDate.Value;
        }
        else
        {
            _logger.LogWarning("Could not determine a valid date for EventId {EventId} notification.", request.EventId);
            return Result<string>.Failure("Could not determine a valid date for notification.", ErrorSources.BadRequest);
        }

        // Determine titles based on gender
        string memberHonorific = "Sự kiện sắp tới";
        string memberName = "";

        var primaryEventMember = @event.EventMembers?.FirstOrDefault();
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
            memberHonorific = $"Sự kiện sắp tới: {@event.Name}";
        }

        // Retrieve recipient user IDs
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
            _logger.LogInformation("No recipients found for EventId {EventId}. Skipping notification.", request.EventId);
            return Result<string>.Success("No recipients found for this event. Notification skipped.");
        }

        // Create payload
        var notificationPayload = new
        {
            event_id = @event.Id.ToString(),
            event_name = @event.Name,
            event_type = @event.Type.ToString(),
            family_id = @event.FamilyId.HasValue ? @event.FamilyId.Value.ToString() : null,
            member_name = memberName,
            lunar_date = @event.LunarDate != null ? $"{@event.LunarDate.Day:D2}/{@event.LunarDate.Month:D2}" : null,
            event_date = notificationDate.ToString("dd/MM"), // Format as dd/MM
            titles = memberHonorific
        };

        // Send notification
        var sendResult = await _notificationService.SendNotificationAsync(
            "event-upcoming", // Use a specific workflow ID for manual sends
            recipientUserIds,
            notificationPayload,
            cancellationToken
        );

        if (sendResult.IsSuccess)
        {
            _logger.LogInformation("Manual notification sent successfully for EventId {EventId} to {RecipientCount} recipients.", request.EventId, recipientUserIds.Count);
            return Result<string>.Success($"Notification sent for EventId {request.EventId} to {recipientUserIds.Count} recipients.");
        }
        else
        {
            _logger.LogError("Failed to send manual notification for EventId {EventId}: {Error}", request.EventId, sendResult.Error);
            return Result<string>.Failure($"Failed to send notification: {sendResult.Error}");
        }
    }
}
