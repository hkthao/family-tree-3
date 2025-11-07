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
    public async Task<Result<string>> CallChatWebhookAsync(string message, List<ChatMessage> history, CancellationToken cancellationToken)
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

        var ollamaMessages = new List<ChatMessage>();
        ollamaMessages.AddRange(history);
        ollamaMessages.Add(new ChatMessage { Role = "user", Content = message });

        var payload = new
        {
            model = n8nSettings.OllamaModel,
            messages = ollamaMessages,
            stream = false
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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            OllamaChatResponse? chatResponse = null;

            // Use a try-catch block to handle different JSON structures (array vs. object)
            try
            {
                // First, try to deserialize as an array of responses
                var responseList = JsonSerializer.Deserialize<List<OllamaChatResponse>>(responseContent, options);
                if (responseList != null && responseList.Count > 0)
                {
                    chatResponse = responseList[0];
                }
            }
            catch (JsonException)
            {
                // If it fails, try to deserialize as a single response object
                chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseContent, options);
            }

            if (chatResponse != null && !string.IsNullOrEmpty(chatResponse.Message?.Content))
            {
                return Result<string>.Success(chatResponse.Message.Content);
            }

            return Result<string>.Failure("Invalid response format from Ollama via n8n.", "ExternalService");
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
        var n8nSettings = _configProvider.GetSection<N8nSettings>();
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

            _logger.LogInformation("Successfully triggered n8n embedding workflow for {EntityType} {EntityId} ({ActionType}).", dto.EntityType, dto.EntityId, dto.ActionType);
            return Result<string>.Success("n8n embedding workflow triggered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while calling the n8n embedding webhook.");
            return Result<string>.Failure($"An error occurred while triggering n8n embedding workflow: {ex.Message}", "Exception");
        }
    }
}
