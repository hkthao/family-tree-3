using System.Net.Http.Json;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Notifications.DTOs; // New using directive
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationSettings _settings; // Changed to NotificationSettings

    public NotificationService(
        HttpClient httpClient,
        ILogger<NotificationService> logger,
        IOptions<NotificationSettings> settings) // Changed to NotificationSettings
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;

        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("NotificationService BaseUrl is not configured, falling back to default."); // Updated message
            _httpClient.BaseAddress = new Uri("http://localhost:3000"); // Fallback for development
        }
        else
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }
    }

    public async Task<Result> SyncSubscriberAsync(SyncSubscriberDto subscriberDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new
            {
                userId = subscriberDto.UserId,
                firstName = subscriberDto.FirstName,
                lastName = subscriberDto.LastName,
                email = subscriberDto.Email,
                phone = subscriberDto.Phone,
                avatar = subscriberDto.Avatar,
                locale = subscriberDto.Locale,
                timezone = subscriberDto.Timezone
            };
            var response = await _httpClient.PostAsJsonAsync("/subscribers/sync", requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully synced subscriber {UserId}. Response: {Response}", subscriberDto.UserId, content);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while syncing subscriber {UserId}.", subscriberDto.UserId);
            return Result.Failure($"Failed to sync subscriber: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> SaveExpoPushTokenAsync(string userId, List<string?> expoPushTokens, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new { userId = userId, expoPushTokens = expoPushTokens };
            var response = await _httpClient.PostAsJsonAsync("/subscribers/expo-token", requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully saved Expo Push Tokens for subscriber {UserId}. Response: {Response}", userId, content);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while saving Expo Push Tokens for subscriber {UserId}.", userId);
            return Result.Failure($"Failed to save Expo Push Tokens: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while saving Expo Push Tokens for subscriber {UserId}.", userId);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> DeleteExpoPushTokenAsync(string userId, string? expoPushToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // For DELETE with body, HttpClient.SendAsync is needed with HttpMethod.Delete
            var request = new HttpRequestMessage(HttpMethod.Delete, "/subscribers/expo-token")
            {
                Content = JsonContent.Create(new { userId = userId, expoPushToken = expoPushToken })
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully deleted Expo Push Token for subscriber {UserId}. Response: {Response}", userId, content);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while deleting Expo Push Token for subscriber {UserId}.", userId);
            return Result.Failure($"Failed to delete Expo Push Token: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while deleting Expo Push Token for subscriber {UserId}.", userId);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> SendNotificationAsync(string workflowId, string userId, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new { workflowId = workflowId, userId = userId, payload = payload };
            var response = await _httpClient.PostAsJsonAsync("/notifications/send", requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            _logger.LogInformation("Successfully sent notification for workflow {WorkflowId} to subscriber {UserId}. Response: {Response}", workflowId, userId, content);
            return Result.Success();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while sending notification for workflow {WorkflowId} to subscriber {UserId}.", workflowId, userId);
            return Result.Failure($"Failed to send notification: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending notification for workflow {WorkflowId} to subscriber {UserId}.", workflowId, userId);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> SendNotificationAsync(string workflowId, List<string> recipientUserIds, object? payload, CancellationToken cancellationToken = default) // NEW Overload
    {
        var allResults = new List<Result>();
        foreach (var userId in recipientUserIds)
        {
            var result = await SendNotificationAsync(workflowId, userId, payload, cancellationToken);
            allResults.Add(result);
        }

        // Return success only if all individual notifications were successful
        if (allResults.All(r => r.IsSuccess))
        {
            return Result.Success();
        }
        else
        {
            // Aggregate errors from failed notifications
            var errorMessages = allResults.Where(r => !r.IsSuccess).Select(r => r.Error).ToList();
            return Result.Failure($"Failed to send to some recipients: {string.Join("; ", errorMessages)}");
        }
    }
}
