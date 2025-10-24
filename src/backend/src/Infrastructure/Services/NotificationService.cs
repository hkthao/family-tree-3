using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services;

public class NotificationService(ILogger<NotificationService> logger, IApplicationDbContext context) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly IApplicationDbContext _context = context;

    public async Task SendNotification(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to send notification to user {RecipientUserId}", message.RecipientUserId);

        var userProfile = await _context.UserProfiles
            .Include(up => up.UserPreference)
            .FirstOrDefaultAsync(up => up.ExternalId == message.RecipientUserId, cancellationToken);

        if (userProfile == null) {
            _logger.LogWarning("User profile not found for recipient {RecipientUserId}. Notification not sent.", message.RecipientUserId);
            return;
        }

        var preferences = userProfile.UserPreference;

        if (preferences == null) {
            _logger.LogWarning("User preferences not found for recipient {RecipientUserId}. Notification not sent.", message.RecipientUserId);
            return;
        }

        if (preferences.EmailNotificationsEnabled) {
            _logger.LogInformation("Sending email notification: Title='{Title}', Body='{Body}' to {RecipientUserId}", message.Title, message.Body, message.RecipientUserId);
            // TODO: Implement actual email sending logic
        }

        if (preferences.InAppNotificationsEnabled) {
            _logger.LogInformation("Sending in-app notification: Title='{Title}', Body='{Body}' to {RecipientUserId}", message.Title, message.Body, message.RecipientUserId);
            // TODO: Implement actual in-app notification logic (e.g., SignalR)
        }

        if (preferences.SmsNotificationsEnabled) {
            _logger.LogInformation("Sending SMS notification: Title='{Title}', Body='{Body}' to {RecipientUserId}", message.Title, message.Body, message.RecipientUserId);
            // TODO: Implement actual SMS sending logic
        }

        if (!preferences.EmailNotificationsEnabled && !preferences.InAppNotificationsEnabled && !preferences.SmsNotificationsEnabled) {
            _logger.LogInformation("No notification channels enabled for user {RecipientUserId}. Notification not sent.", message.RecipientUserId);
        }
    }
}
