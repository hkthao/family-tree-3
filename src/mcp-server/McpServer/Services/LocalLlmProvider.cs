using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
namespace McpServer.Services;
/// <summary>
/// Nhà cung cấp AI sử dụng Local LLM (ví dụ: Ollama), có khả năng giả lập tool-calling.
/// </summary>
public class LocalLlmProvider : IAiProvider
{
    private readonly LocalLlmSettings _settings;
    private readonly ILogger<LocalLlmProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAiPromptBuilder _promptBuilder;
    public LocalLlmProvider(IOptions<LocalLlmSettings> settings, ILogger<LocalLlmProvider> logger, HttpClient httpClient, IAiPromptBuilder promptBuilder)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;
        _promptBuilder = promptBuilder;
    }

    public async IAsyncEnumerable<AiResponsePart> GenerateToolUseResponseStreamAsync(
        string prompt,
        List<AiToolDefinition>? tools = null,
        List<AiToolResult>? toolResults = null)
    {
        var fullPrompt = _promptBuilder.BuildPromptForToolUse(prompt, tools, toolResults);
        var parts = new List<AiResponsePart>();
        var requestBody = new
        {
            model = _settings.Model,
            prompt = fullPrompt,
            stream = true,
            format = "json" // Yêu cầu Ollama trả về JSON
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/generate") // Use BaseAddress directly
        {
            Content = JsonContent.Create(requestBody)
        };
        try
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);
            var fullTextResponse = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("response", out var responseProperty))
                    {
                        fullTextResponse.Append(responseProperty.GetString());
                    }
                    // We no longer check for "tool_calls" directly here, as Ollama fragments it within "response"
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse JSON chunk from Local LLM stream: {Line}", line);
                }
            }
            // After the stream ends, try to parse the accumulated fullTextResponse for tool calls
            if (fullTextResponse.Length > 0)
            {
                try
                {
                    using var doc = JsonDocument.Parse(fullTextResponse.ToString());
                    if (doc.RootElement.TryGetProperty("tool_calls", out var toolCallsElement) && toolCallsElement.ValueKind == JsonValueKind.Array)
                    {
                        var toolCalls = new List<AiToolCall>();
                        foreach (var toolCallElement in toolCallsElement.EnumerateArray())
                        {
                            var id = toolCallElement.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                            var function = toolCallElement.GetProperty("function");
                            var name = function.GetProperty("name").GetString() ?? string.Empty;
                            var args = function.GetProperty("arguments").GetString() ?? string.Empty;
                            toolCalls.Add(new AiToolCall(id, name, args));
                        }
                        parts.Add(new AiToolCallResponsePart(toolCalls));
                    }
                    else
                    {
                        // If it's valid JSON but not tool calls, treat as text
                        parts.Add(new AiTextResponsePart(fullTextResponse.ToString()));
                    }
                }
                catch (JsonException)
                {
                    // If it's not valid JSON, treat the whole thing as plain text
                    parts.Add(new AiTextResponsePart(fullTextResponse.ToString()));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Local LLM API: {Message}", ex.Message);
            parts.Add(new AiTextResponsePart($"Error calling Local LLM: {ex.Message}"));
        }
        foreach (var part in parts)
        {
            yield return part;
        }
    }
   
    public async IAsyncEnumerable<AiResponsePart> GenerateChatResponseStreamAsync(string prompt)
    {
        var fullPrompt = _promptBuilder.BuildPromptForChat(prompt);
        var parts = new List<AiResponsePart>();
        var requestBody = new
        {
            model = _settings.Model,
            prompt = fullPrompt,
            stream = true,
            format = "json" // Yêu cầu Ollama trả về JSON
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/chat") // Use BaseAddress directly
        {
            Content = JsonContent.Create(requestBody)
        };
        try
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);
            var fullTextResponse = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("response", out var responseProperty))
                    {
                        fullTextResponse.Append(responseProperty.GetString());
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse JSON chunk from Local LLM stream: {Line}", line);
                }
            }
            if (fullTextResponse.Length > 0)
            {
                parts.Add(new AiTextResponsePart(fullTextResponse.ToString()));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Local LLM API: {Message}", ex.Message);
            parts.Add(new AiTextResponsePart($"Error calling Local LLM: {ex.Message}"));
        }
        foreach (var part in parts)
        {
            yield return part;
        }
    }

    public async Task<string> GetStatusAsync()
    {
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            return "Local LLM BaseUrl is not configured.";
        }
        try
        {
            // Assuming a simple GET request to the base URL can indicate status
            var response = await _httpClient.GetAsync("/");
            response.EnsureSuccessStatusCode();
            return "Local LLM is operational.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error checking Local LLM status.");
            return $"Local LLM is not reachable. {ex.Message}";
        }
    }
}
