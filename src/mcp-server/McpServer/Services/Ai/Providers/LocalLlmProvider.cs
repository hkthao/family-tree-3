using System.Text.Json;
using McpServer.Config;
using McpServer.Models;
using McpServer.Services.Ai.AITools;
using Microsoft.Extensions.Options;

namespace McpServer.Services.Ai.Providers;

/// <summary>
/// Provides AI services using a local LLM (e.g., Ollama).
/// </summary>
public class LocalLlmProvider : IAiProvider
{
    private readonly LocalLlmSettings _settings;
    private readonly ILogger<LocalLlmProvider> _logger;
    private readonly HttpClient _httpClient;

    public LocalLlmProvider(IOptions<LocalLlmSettings> settings, ILogger<LocalLlmProvider> logger, HttpClient httpClient)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;

        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _logger.LogWarning("Local LLM BaseUrl is not configured.");
        }
    }

    /// <summary>
    /// Generates a response from the local LLM, potentially including tool calls.
    /// </summary>
    /// <param name="messages">The conversation history and current prompt.</param>
    /// <param name="toolRegistry">The registry of available tools.</param>
    /// <returns>An async enumerable of AI response parts (text or tool calls).</returns>
    public async IAsyncEnumerable<AiResponsePart> GenerateToolUseResponseStreamAsync(List<AiMessage> messages, ToolRegistry toolRegistry)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            yield return AiResponsePart.FromText("Error: Local LLM BaseUrl is not configured.");
            yield break;
        }

        // Simplified request for local LLM, assuming it can handle tool definitions in a specific format
        // This part might need significant adjustment based on the actual local LLM API.
        var requestBody = new
        {
            model = _settings.Model, // e.g., "llama3"
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            tools = toolRegistry.Tools.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.Parameters
                }
            }),
            stream = true
        };

        var requestJson = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _logger.LogInformation("Sending request to Local LLM: {RequestJson}", requestJson);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/chat")
        {
            Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var responseParts = new List<AiResponsePart>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var jsonDoc = JsonDocument.Parse(line);
                if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    if (messageElement.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.String)
                    {
                        responseParts.Add(AiResponsePart.FromText(contentElement.GetString() ?? string.Empty));
                    }
                    if (messageElement.TryGetProperty("tool_calls", out var toolCallsElement) && toolCallsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var toolCallElement in toolCallsElement.EnumerateArray())
                        {
                            var id = toolCallElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : Guid.NewGuid().ToString();
                            var functionElement = toolCallElement.GetProperty("function");
                            var name = functionElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : string.Empty;
                            var arguments = functionElement.TryGetProperty("arguments", out var argsElement) ? argsElement.GetRawText() : string.Empty;

                            var toolCall = new AiToolCall
                            {
                                Id = id ?? Guid.NewGuid().ToString(),
                                Function = new AiToolFunction
                                {
                                    Name = name ?? string.Empty,
                                    Arguments = arguments ?? string.Empty
                                }
                            };
                            responseParts.Add(AiResponsePart.FromToolCall(toolCall));
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing Local LLM response line: {Line}", line);
                responseParts.Add(AiResponsePart.FromText($"Error parsing AI response: {ex.Message}"));
            }
        }

        foreach (var part in responseParts)
        {
            yield return part;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="prompt">The user's prompt.</param>
    /// <returns>An async enumerable of AI response parts (text only).</returns>
    public async IAsyncEnumerable<AiResponsePart> GenerateChatResponseStreamAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            yield return AiResponsePart.FromText("Error: Local LLM BaseUrl is not configured.");
            yield break;
        }

        var requestBody = new
        {
            model = _settings.Model, // e.g., "llama3"
            messages = new[] { new { role = "user", content = prompt } },
            stream = true
        };

        var requestJson = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _logger.LogInformation("Sending chat request to Local LLM: {RequestJson}", requestJson);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/api/chat")
        {
            Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var responseParts = new List<AiResponsePart>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var jsonDoc = JsonDocument.Parse(line);
                if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement) && messageElement.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.String)
                {
                    responseParts.Add(AiResponsePart.FromText(contentElement.GetString() ?? string.Empty));
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing Local LLM chat response line: {Line}", line);
                responseParts.Add(AiResponsePart.FromText($"Error parsing AI chat response: {ex.Message}"));
            }
        }

        foreach (var part in responseParts)
        {
            yield return part;
        }
    }

    /// <summary>
    /// Checks the status of the local LLM API.
    /// </summary>
    /// <returns>The status of the API.</returns>
    public async Task<string> GetStatusAsync()
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            return "Local LLM BaseUrl is not configured.";
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl}/api/tags"); // Example endpoint for Ollama
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return "Local LLM is accessible.";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Local LLM returned an error: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            return $"Error checking Local LLM status: {ex.Message}";
        }
    }
}