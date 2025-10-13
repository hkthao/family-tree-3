using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace backend.Infrastructure.AI.Chat;

public class LocalChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly AIChatSettings _chatSettings;
    private readonly ILogger<LocalChatProvider> _logger;

    public LocalChatProvider(HttpClient httpClient, AIChatSettings chatSettings, ILogger<LocalChatProvider> logger)
    {
        _httpClient = httpClient;
        _chatSettings = chatSettings;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(List<ChatMessage> messages)
    {
        if (string.IsNullOrWhiteSpace(_chatSettings.Local.ApiUrl))
        {
            _logger.LogError("Ollama chat API URL is not configured.");
            return "Error: Ollama chat API URL is not configured.";
        }
        if (string.IsNullOrWhiteSpace(_chatSettings.Local.Model))
        {
            _logger.LogError("Ollama chat model is not configured.");
            return "Error: Ollama chat model is not configured.";
        }

        try
        {
            var requestBody = new
            {
                model = _chatSettings.Local.Model,
                messages,
                stream = false
            };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_chatSettings.Local.ApiUrl, content);
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
