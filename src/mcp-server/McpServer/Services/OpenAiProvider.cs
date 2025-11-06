using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

namespace McpServer.Services
{
    /// <summary>
    /// Nhà cung cấp AI sử dụng OpenAI API, có hỗ trợ tool-calling.
    /// </summary>
    public class OpenAiProvider : IAiProvider
    {
        private readonly OpenAiSettings _settings;
        private readonly ILogger<OpenAiProvider> _logger;
        private readonly HttpClient _httpClient;
        private readonly IAiPromptBuilder _promptBuilder; // Inject IAiPromptBuilder

        public OpenAiProvider(IOptions<OpenAiSettings> settings, ILogger<OpenAiProvider> logger, HttpClient httpClient, IAiPromptBuilder promptBuilder)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClient;
            _promptBuilder = promptBuilder;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/"); // OpenAI Base URL
        }

        public async IAsyncEnumerable<AiResponsePart> GenerateToolUseResponseStreamAsync(
            string prompt,
            List<AiToolDefinition>? tools = null,
            List<AiToolResult>? toolResults = null)
        {
            var fullPrompt = _promptBuilder.BuildPromptForToolUse(prompt, tools, toolResults);
            List<AiResponsePart> parts = new List<AiResponsePart>();
            var messages = new List<object>
            {
                new { role = "user", content = fullPrompt }
            };

            if (toolResults != null && toolResults.Any())
            {
                foreach (var toolResult in toolResults)
                {
                    messages.Add(new
                    {
                        role = "tool",
                        tool_call_id = toolResult.ToolCallId,
                        content = toolResult.Content
                    });
                }
            }

            var requestBody = new
            {
                model = _settings.Model,
                messages = messages,
                temperature = 0.7,
                max_tokens = 1024,
                top_p = 0.95,
                frequency_penalty = 0,
                presence_penalty = 0,
                stream = true,
                tools = tools?.Select(t => new { type = "function", function = t }).ToList() // OpenAI expects 'type: function'
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(requestBody)
            };

            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(responseStream);

                var toolCallBuffer = new StringBuilder();
                var isToolCall = false;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.StartsWith("data: "))
                    {
                        var data = line.Substring("data: ".Length);
                        if (data == "[DONE]")
                        {
                            break;
                        }

                        using var doc = JsonDocument.Parse(data);
                        var choices = doc.RootElement.GetProperty("choices");
                        if (choices.GetArrayLength() > 0)
                        {
                            var choice = choices[0];
                            var delta = choice.GetProperty("delta");

                            // Check for tool calls
                            if (delta.TryGetProperty("tool_calls", out var toolCallsElement) && toolCallsElement.ValueKind == JsonValueKind.Array)
                            {
                                isToolCall = true;
                                foreach (var toolCallDelta in toolCallsElement.EnumerateArray())
                                {
                                    toolCallBuffer.Append(toolCallDelta.GetRawText());
                                }
                            }
                            else if (delta.TryGetProperty("content", out var contentElement))
                            {
                                if (isToolCall && toolCallBuffer.Length > 0) // End of tool call, yield it
                                {
                                    var openAiToolCalls = ParseOpenAiToolCalls(toolCallBuffer.ToString());
                                    if (openAiToolCalls != null && openAiToolCalls.Any())
                                    {
                                        parts.Add(new AiToolCallResponsePart(openAiToolCalls));
                                    }
                                    toolCallBuffer.Clear();
                                    isToolCall = false;
                                }
                                parts.Add(new AiTextResponsePart(contentElement.GetString() ?? string.Empty));
                            }
                        }
                    }
                }

                // Yield any remaining tool call buffer at the end of the stream
                if (isToolCall && toolCallBuffer.Length > 0)
                {
                    var openAiToolCalls = ParseOpenAiToolCalls(toolCallBuffer.ToString());
                    if (openAiToolCalls != null && openAiToolCalls.Any())
                    {
                        parts.Add(new AiToolCallResponsePart(openAiToolCalls));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API: {Message}", ex.Message);
                parts.Add(new AiTextResponsePart($"Error calling OpenAI: {ex.Message}"));
            }

            foreach (var part in parts)
            {
                yield return part;
            }
        }

        public async IAsyncEnumerable<AiResponsePart> GenerateChatResponseStreamAsync(string prompt)
        {
            var fullPrompt = _promptBuilder.BuildPromptForChat(prompt);
            List<AiResponsePart> parts = new List<AiResponsePart>();
            var messages = new List<object>
            {
                new { role = "user", content = fullPrompt }
            };

            var requestBody = new
            {
                model = _settings.Model,
                messages = messages,
                temperature = 0.7,
                max_tokens = 1024,
                top_p = 0.95,
                frequency_penalty = 0,
                presence_penalty = 0,
                stream = true,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(requestBody)
            };

            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(responseStream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.StartsWith("data: "))
                    {
                        var data = line.Substring("data: ".Length);
                        if (data == "[DONE]")
                        {
                            break;
                        }

                        using var doc = JsonDocument.Parse(data);
                        var choices = doc.RootElement.GetProperty("choices");
                        if (choices.GetArrayLength() > 0)
                        {
                            var choice = choices[0];
                            var delta = choice.GetProperty("delta");

                            if (delta.TryGetProperty("content", out var contentElement))
                            {
                                parts.Add(new AiTextResponsePart(contentElement.GetString() ?? string.Empty));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API: {Message}", ex.Message);
                parts.Add(new AiTextResponsePart($"Error calling OpenAI: {ex.Message}"));
            }

            foreach (var part in parts)
            {
                yield return part;
            }
        }

        private List<AiToolCall>? ParseOpenAiToolCalls(string toolCallJson)
        {
            try
            {
                // OpenAI sends tool calls as a list of objects, each with id, type, function { name, arguments }
                // The 'arguments' field is a JSON string itself.
                var doc = JsonDocument.Parse($"[{{toolCallJson}}]"); // Wrap in array for easier parsing if it's a single object
                var toolCalls = new List<AiToolCall>();

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    var id = element.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                    var function = element.GetProperty("function");
                    var name = function.GetProperty("name").GetString() ?? string.Empty;
                    var args = function.GetProperty("arguments").GetString() ?? string.Empty; // Arguments are already a JSON string

                    toolCalls.Add(new AiToolCall(id, name, args));
                }
                return toolCalls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI tool call JSON: {Json}", toolCallJson);
                return null;
            }
        }

        public Task<string> GetStatusAsync()
        {
            // In a real scenario, you might ping the OpenAI API or check credentials.
            return Task.FromResult("OpenAI is operational.");
        }
    }
}