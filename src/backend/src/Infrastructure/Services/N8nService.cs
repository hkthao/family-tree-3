using System.Text;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AI;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai dịch vụ để tương tác với n8n.
/// </summary>
public class N8nService : IN8nService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigProvider _configProvider;
    private readonly ILogger<N8nService> _logger;

    public N8nService(IHttpClientFactory httpClientFactory, IConfigProvider configProvider, ILogger<N8nService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configProvider = configProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<string>> CallChatWebhookAsync(string sessionId, string message, CancellationToken cancellationToken)
    {
        var n8nSettings = _configProvider.GetSection<N8nSettings>();
        if (string.IsNullOrEmpty(n8nSettings.ChatWebhookUrl) || n8nSettings.ChatWebhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n chat webhook URL is not configured.");
            return Result<string>.Failure("n8n chat integration is not configured.", "Configuration");
        }

        if (string.IsNullOrEmpty(n8nSettings.OllamaModel))
        {
            _logger.LogWarning("Ollama model is not configured in N8nSettings.");
            return Result<string>.Failure("Ollama model is not configured.", "Configuration");
        }

        var httpClient = _httpClientFactory.CreateClient();

        var payload = new[]
        {
            new
            {
                sessionId = sessionId,
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
            _logger.LogInformation("Received successful response from n8n webhook.");
            _logger.LogDebug("Raw n8n chat webhook response: {ResponseContent}", responseContent); // Log raw response

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
                _logger.LogError(ex, "Failed to deserialize n8n chat webhook response as ChatResponse.");
                return Result<string>.Failure("Invalid response format from n8n: Failed to deserialize chat response.", "ExternalService");
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
    public async Task<Result<double[]>> CallEmbeddingWebhookAsync(EmbeddingWebhookDto dto, CancellationToken cancellationToken)
    {
        var n8nSettings = _configProvider.GetSection<N8nSettings>();
        if (string.IsNullOrEmpty(n8nSettings.EmbeddingWebhookUrl) || n8nSettings.EmbeddingWebhookUrl == "YOUR_N8N_WEBHOOK_URL_HERE")
        {
            _logger.LogWarning("n8n embedding webhook URL is not configured.");
            return Result<double[]>.Failure("n8n embedding integration is not configured.", "Configuration");
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
                return Result<double[]>.Failure($"Failed to trigger n8n embedding workflow. Status: {response.StatusCode}", "ExternalService");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Received from n8n embedding webhook: {ResponseContent}", responseContent);

            // Assuming n8n returns a JSON object with an 'embedding' property that is an array of doubles
            var jsonDocument = JsonDocument.Parse(responseContent);
            if (jsonDocument.RootElement.TryGetProperty("embedding", out var embeddingElement) && embeddingElement.ValueKind == JsonValueKind.Array)
            {
                var embedding = embeddingElement.EnumerateArray().Select(e => e.GetDouble()).ToArray();
                _logger.LogInformation("Successfully received embedding from n8n for {EntityType} {EntityId} ({ActionType}).", dto.EntityType, dto.EntityId, dto.ActionType);
                return Result<double[]>.Success(embedding);
            }
            else
            {
                _logger.LogError("Invalid response format from n8n embedding webhook: 'embedding' property not found or not an array.");
                return Result<double[]>.Failure("Invalid response format from n8n embedding webhook.", "ExternalService");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n embedding webhook.");
            return Result<double[]>.Failure($"An error occurred while triggering n8n embedding workflow: {ex.Message}", "Exception");
        }
    }
}
