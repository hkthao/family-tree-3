using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Headers;
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

    public LocalLlmProvider(IOptions<LocalLlmSettings> settings, ILogger<LocalLlmProvider> logger, HttpClient httpClient)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;
    }

            public async IAsyncEnumerable<AiResponsePart> GenerateResponseStreamAsync(
                string prompt,
                List<AiToolDefinition>? tools = null,
                List<AiToolResult>? toolResults = null)
            {
                var fullPrompt = BuildPrompt(prompt, tools, toolResults);
                var parts = new List<AiResponsePart>();
    
                var requestBody = new
                {
                    model = _settings.Model,
                    prompt = fullPrompt,
                    stream = true,
                    format = "json" // Yêu cầu Ollama trả về JSON
                };
    
                var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress) // Use BaseAddress directly
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
                    bool toolCallDetected = false;
    
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line)) continue;
    
                        try
                        {
                            using var doc = JsonDocument.Parse(line);
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
                                toolCallDetected = true;
                            }
                            else if (doc.RootElement.TryGetProperty("response", out var responseProperty))
                            {
                                fullTextResponse.Append(responseProperty.GetString());
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse JSON chunk from Local LLM stream: {Line}", line);
                        }
                    }
    
                    if (!toolCallDetected && fullTextResponse.Length > 0)
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

    private string BuildPrompt(string userPrompt, List<AiToolDefinition>? tools, List<AiToolResult>? toolResults)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful assistant with access to the following tools.");
        sb.AppendLine("To use a tool, you must respond with a JSON object matching the following schema:");
        sb.AppendLine("{\"tool_calls\": [{\"id\": \"<unique_id>\", \"function\": {\"name\": \"<tool_name>\", \"arguments\": \"<json_escaped_arguments>\"}}]}");
        sb.AppendLine("If you don't need to use a tool, just respond with a regular text message.");
        sb.AppendLine("\nIMPORTANT TOOL USAGE GUIDELINES:");
        sb.AppendLine("- Use the 'search_family' tool when the user provides a family name, keyword, or any general query to find families.");
        sb.AppendLine("- ONLY use the 'get_family_details' tool when you have a specific, confirmed family ID (GUID). Do NOT use it with a family name or general query.");
        sb.AppendLine("\nHere are the available tools:");
        sb.AppendLine(JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true }));

        if (toolResults != null && toolResults.Any())
        {
            sb.AppendLine("\nYou have previously called tools and received these results:");
            sb.AppendLine(JsonSerializer.Serialize(toolResults, new JsonSerializerOptions { WriteIndented = true }));
            sb.AppendLine("\nBased on these results, please provide a final answer to the user's original query. DO NOT call any more tools.");
        }

        sb.AppendLine("\nUser Query:");
        sb.AppendLine(userPrompt);

        return sb.ToString();
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
