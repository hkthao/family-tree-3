using backend.Application.Common.Events; // New using directive for events
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.Notifications.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

public class NotificationService(
    ILogger<NotificationService> logger,
    IMessageBus messageBus) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly IMessageBus _messageBus = messageBus;

    public async Task<Result> SyncSubscriberAsync(SyncSubscriberDto subscriberDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userSyncEvent = new UserSyncEvent(
                subscriberDto.UserId,
                subscriberDto.FirstName,
                subscriberDto.LastName,
                subscriberDto.Email,
                subscriberDto.Phone,
                subscriberDto.Avatar,
                subscriberDto.Locale,
                subscriberDto.Timezone
            );
            await _messageBus.PublishAsync("notification", "user.sync", userSyncEvent, cancellationToken);
            _logger.LogInformation("Published UserSyncEvent for UserId: {UserId}", subscriberDto.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserSyncEvent for UserId: {UserId}", subscriberDto.UserId);
            return Result.Failure($"Failed to sync subscriber via message bus: {ex.Message}");
        }
    }

    public async Task<Result> SaveExpoPushTokenAsync(string userId, List<string?> expoPushTokens, CancellationToken cancellationToken = default)
    {
        try
        {
            var saveExpoPushTokenEvent = new SaveExpoPushTokenEvent(userId, expoPushTokens);
            await _messageBus.PublishAsync("notification", "expo.save", saveExpoPushTokenEvent, cancellationToken);
            _logger.LogInformation("Published SaveExpoPushTokenEvent for UserId: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SaveExpoPushTokenEvent for UserId: {UserId}", userId);
            return Result.Failure($"Failed to save Expo Push Tokens via message bus: {ex.Message}");
        }
    }

    public async Task<Result> DeleteExpoPushTokenAsync(string userId, string? expoPushToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteExpoPushTokenEvent = new DeleteExpoPushTokenEvent(userId, expoPushToken);
            await _messageBus.PublishAsync("notification", "expo.delete", deleteExpoPushTokenEvent, cancellationToken);
            _logger.LogInformation("Published DeleteExpoPushTokenEvent for UserId: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish DeleteExpoPushTokenEvent for UserId: {UserId}", userId);
            return Result.Failure($"Failed to delete Expo Push Token via message bus: {ex.Message}");
        }
    }

    public async Task<Result> SendNotificationAsync(string workflowId, string userId, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var sendNotificationEvent = new SendNotificationEvent(workflowId, userId, payload);
            await _messageBus.PublishAsync("notification", $"notification.send.{workflowId}", sendNotificationEvent, cancellationToken);
            _logger.LogInformation("Published SendNotificationEvent for WorkflowId: {WorkflowId}, UserId: {UserId}.", workflowId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SendNotificationEvent for WorkflowId: {WorkflowId}, UserId: {UserId}", workflowId, userId);
            return Result.Failure($"Failed to send notification via message bus: {ex.Message}");
        }
    }

    public async Task<Result> SendNotificationAsync(string workflowId, List<string> recipientUserIds, object? payload, CancellationToken cancellationToken = default)
    {
        var allResults = new List<Result>();
        foreach (var userId in recipientUserIds)
        {
            // The single recipient overload will extract familyId from the payload
            var result = await SendNotificationAsync(workflowId, userId, payload, cancellationToken);
            allResults.Add(result);
        }

        // Return success only if all individual notifications were successful
        if (allResults.All(r => r.IsSuccess))
        {
            _logger.LogInformation("Successfully sent notifications for workflow {WorkflowId} to {RecipientCount} recipients.", workflowId, recipientUserIds.Count);
            return Result.Success();
        }
        else
        {
            // Aggregate errors from failed notifications
            var errorMessages = allResults.Where(r => !r.IsSuccess).Select(r => r.Error).ToList();
            _logger.LogError("Failed to send notifications for workflow {WorkflowId} to some recipients. Errors: {Errors}", workflowId, string.Join("; ", errorMessages));
            return Result.Failure($"Failed to send to some recipients: {string.Join("; ", errorMessages)}");
        }
    }
}
