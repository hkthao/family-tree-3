using System.Text;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AI;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai dịch vụ để tương tác với n8n.
/// </summary>
public class N8nService : IN8nService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<N8nService> _logger;

    public N8nService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<N8nService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<string>> CallChatWebhookAsync(string sessionId, string message, CancellationToken cancellationToken)
    {
        var n8nSettings = _configuration.GetSection(nameof(N8nSettings)).Get<N8nSettings>() ?? new N8nSettings();
        if (string.IsNullOrEmpty(n8nSettings.ChatWebhookUrl) || n8nSettings.ChatWebhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n chat webhook URL is not configured.");
            return Result<string>.Failure("n8n chat integration is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        var payload = new[]
        {
            new
            {
                sessionId,
                action = "sendMessage",
                chatInput = message
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n chat webhook at {Url}", n8nSettings.ChatWebhookUrl);
            var response = await httpClient.PostAsync(n8nSettings.ChatWebhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Failed to get response from AI assistant. Status: {response.StatusCode}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n: {ResponseContent}", responseContent);
            _logger.LogDebug("Raw n8n chat webhook response: {ResponseContent}", responseContent); // Log raw response
            _logger.LogInformation("Received successful response from n8n webhook.");
            _logger.LogDebug("Raw n8n chat webhook response: {ResponseContent}", responseContent); // Log raw response

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("Received empty response content from n8n chat webhook.");
                return Result<string>.Failure("Invalid response format from n8n: Received empty response.", "ExternalService");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            ChatResponse? chatResponse = null;

            try
            {
                chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize n8n chat webhook response as ChatResponse. Raw response: {RawResponse}", responseContent);
                return Result<string>.Failure($"Invalid response format from n8n: Failed to deserialize chat response. Raw response: {responseContent}", "ExternalService");
            }

            if (chatResponse != null && !string.IsNullOrEmpty(chatResponse.Output))
            {
                return Result<string>.Success(chatResponse.Output ?? string.Empty);
            }

            return Result<string>.Failure("Invalid response format from n8n: Empty or invalid chat response.", "ExternalService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n webhook.");
            return Result<string>.Failure("An error occurred: {ex.Message}", "Exception");
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> CallEmbeddingWebhookAsync(EmbeddingWebhookDto dto, CancellationToken cancellationToken)
    {
        var n8nSettings = _configuration.GetSection(nameof(N8nSettings)).Get<N8nSettings>() ?? new N8nSettings();
        if (string.IsNullOrEmpty(n8nSettings.EmbeddingWebhookUrl) || n8nSettings.EmbeddingWebhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n embedding webhook URL is not configured.");
            return Result<string>.Failure("n8n embedding integration is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        var payload = new
        {
            dto.EntityType,
            dto.EntityId,
            dto.ActionType,
            dto.EntityData,
            dto.Description
        };

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Calling n8n embedding webhook at {Url} with payload: {Payload}", n8nSettings.EmbeddingWebhookUrl, jsonPayload);
            var response = await httpClient.PostAsync(n8nSettings.EmbeddingWebhookUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to call n8n embedding webhook. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                return Result<string>.Failure($"Failed to trigger n8n embedding workflow. Status: {response.StatusCode}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n embedding webhook: {ResponseContent}", responseContent);

            // Assuming n8n returns a JSON object with a 'result' property, which contains a 'points' array,
            // and each point has a 'payload' with a 'member_id' or 'memberId' property.
            var jsonDocument = JsonDocument.Parse(responseContent);
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (jsonDocument.RootElement.TryGetProperty("result", out var resultElement) && resultElement.TryGetProperty("points", out var pointsElement) && pointsElement.ValueKind == JsonValueKind.Array && pointsElement.GetArrayLength() > 0)
                {
                    var firstPoint = pointsElement[0];
                    if (firstPoint.TryGetProperty("payload", out var payloadElement))
                    {
                        // Try to get "member_id" (snake_case) first, then "memberId" (camelCase)
                        if (payloadElement.TryGetProperty("member_id", out var memberIdElement) && memberIdElement.ValueKind == JsonValueKind.String)
                        {
                            var memberId = memberIdElement.GetString();
                            _logger.LogInformation("Successfully received memberId '{MemberId}' from n8n for {EntityType} {EntityId} ({ActionType}).", memberId, dto.EntityType, dto.EntityId, dto.ActionType);
                            return Result<string>.Success(memberId ?? string.Empty);
                        }
                        else if (payloadElement.TryGetProperty("memberId", out memberIdElement) && memberIdElement.ValueKind == JsonValueKind.String)
                        {
                            var memberId = memberIdElement.GetString();
                            _logger.LogInformation("Successfully received memberId '{MemberId}' from n8n for {EntityType} {EntityId} ({ActionType}).", memberId, dto.EntityType, dto.EntityId, dto.ActionType);
                            return Result<string>.Success(memberId ?? string.Empty);
                        }
                    }
                }
            }

            _logger.LogInformation("No memberId found or invalid format in n8n embedding webhook response for {EntityType} {EntityId} ({ActionType}).", dto.EntityType, dto.EntityId, dto.ActionType);
            return Result<string>.Success(string.Empty); // No member found, return empty string
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n embedding webhook.");
            return Result<string>.Failure($"An error occurred while triggering n8n embedding workflow: {ex.Message}", "Exception");
        }
    }
}
