using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.AI.Chat;

public class LocalChatProvider(HttpClient httpClient, IConfigProvider configProvider, ILogger<LocalChatProvider> logger) : IChatProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<LocalChatProvider> _logger = logger;
    private readonly IConfigProvider _configProvider = configProvider;

    public async Task<string> GenerateResponseAsync(List<ChatMessage> messages)
    {
        var chatSettings = _configProvider.GetSection<AIChatSettings>();
        if (string.IsNullOrWhiteSpace(chatSettings.Local.ApiUrl))
        {
            _logger.LogError("Ollama chat API URL is not configured.");
            return "Error: Ollama chat API URL is not configured.";
        }
        if (string.IsNullOrWhiteSpace(chatSettings.Local.Model))
        {
            _logger.LogError("Ollama chat model is not configured.");
            return "Error: Ollama chat model is not configured.";
        }

        try
        {
            var requestBody = new
            {
                model = chatSettings.Local.Model,
                messages,
                stream = false,
                max_tokens = 300
            };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(chatSettings.Local.ApiUrl, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("message", out JsonElement messageElement) &&
                    messageElement.TryGetProperty("content", out JsonElement contentElement))
                {
                    return contentElement.GetString() ?? string.Empty;
                }
            }

            _logger.LogError("Failed to parse chat response from Ollama API.");
            return "Error: Failed to parse chat response from Ollama API.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ollama chat API request failed: {Message}", ex.Message);
            return $"Error: Ollama chat API request failed: {ex.Message}";
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Ollama chat API response: {Message}", ex.Message);
            return $"Error: Failed to deserialize Ollama chat API response: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during Ollama chat generation: {Message}", ex.Message);
            return $"Error: An unexpected error occurred during Ollama chat generation: {ex.Message}";
        }
    }
}
