using System.Text.Json;

namespace McpServer.Services;

/// <summary>
/// Dịch vụ chính để tương tác với AI, điều phối luồng tool-use.
/// </summary>
public class AiService
{
    private readonly AiProviderFactory _aiProviderFactory;
    private readonly ILogger<AiService> _logger;
    private readonly string _defaultAiProvider;
    private readonly ToolExecutor _toolExecutor; // Inject ToolExecutor

    public AiService(
        AiProviderFactory aiProviderFactory,
        ILogger<AiService> logger,
        IConfiguration configuration,
        ToolExecutor toolExecutor) // Inject ToolExecutor
    {
        _aiProviderFactory = aiProviderFactory;
        _logger = logger;
        _defaultAiProvider = configuration["DefaultAiProvider"] ?? "Gemini";
        _toolExecutor = toolExecutor; // Assign ToolExecutor
    }

    /// <summary>
    /// Lấy phản hồi từ AI, có thể qua nhiều bước gọi tool.
    /// </summary>
    public async IAsyncEnumerable<string> GetAiResponseStreamAsync(string prompt, string? jwtToken, string? providerName = null)
    {
        List<string> finalResponseChunks = new List<string>();
        var selectedProviderName = providerName ?? _defaultAiProvider;
        IAiProvider? aiProvider = null;
        try
        {
            aiProvider = _aiProviderFactory.GetProvider(selectedProviderName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid AI provider specified: {ProviderName}", selectedProviderName);
            finalResponseChunks.Add($"Error: Invalid AI provider '{selectedProviderName}'.");
        }

        if (aiProvider == null)
        {
            foreach (var chunk in finalResponseChunks) yield return chunk;
            yield break;
        }
        // 1. Định nghĩa các tool có sẵn
        var tools = DefineTools();

        // 2. Gửi prompt và danh sách tool đến LLM
        _logger.LogInformation("Phase 1: Sending prompt and tool definitions to LLM.");
        var responseParts = aiProvider.GenerateResponseStreamAsync(prompt, tools);

        var toolCalls = new List<AiToolCall>();
        await foreach (var part in responseParts)
        {
            _logger.LogInformation("Received AiResponsePart of type: {PartType}", part.GetType().Name);
            if (part is AiTextResponsePart textPart)
            {
                // Nếu LLM trả lời thẳng bằng text (không gọi tool), stream nó ra ngay
                _logger.LogInformation("LLM responded with text directly. Streaming response.");
                finalResponseChunks.Add(textPart.Text);
            }
            else if (part is AiToolCallResponsePart toolCallPart)
            {
                _logger.LogInformation("LLM requested to call {ToolCount} tools.", toolCallPart.ToolCalls.Count);
                toolCalls.AddRange(toolCallPart.ToolCalls);
            }
        }

        // 3. Nếu không có tool nào được gọi, kết thúc
        if (toolCalls.Count == 0)
        {
            _logger.LogInformation("No tools were called by the LLM. Finishing.");
            foreach (var chunk in finalResponseChunks) yield return chunk;
            yield break;
        }

        // 4. Thực thi các tool call
        var toolResults = new List<AiToolResult>();
        foreach (var toolCall in toolCalls)
        {
            // Delegate tool execution to ToolExecutor
            var toolResult = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);
            toolResults.Add(toolResult);
        }

        // 5. Gửi kết quả tool call lại cho LLM để tổng hợp câu trả lời
        _logger.LogInformation("Phase 2: Sending tool results back to LLM for final response.");

        var finalResponseParts = aiProvider.GenerateResponseStreamAsync(prompt, tools, toolResults);

        await foreach (var part in finalResponseParts)
        {
            if (part is AiTextResponsePart textPart)
            {
                finalResponseChunks.Add(textPart.Text);
            }
            else if (part is AiToolCallResponsePart toolCallPart)
            {
                if (toolCallPart.ToolCalls.Any())
                {
                    _logger.LogError("LLM attempted to call tools in the final response phase. This should not happen.");
                    finalResponseChunks.Add("Error: The AI tried to call a tool again unexpectedly. Please try rephrasing your request.");
                }
            }
        }
        _logger.LogInformation("Finished streaming final response from LLM.");
        foreach (var chunk in finalResponseChunks) yield return chunk;
    }

    private List<AiToolDefinition> DefineTools()
    {
        return new List<AiToolDefinition>
        {
            new AiToolDefinition
            {
                Name = "search_family",
                Description = "Search for family information by name, code, or ID.",
                Parameters = new AiToolParameters
                {
                    Properties = new Dictionary<string, AiToolParameterProperty>
                    {
                        ["query"] = new AiToolParameterProperty { Type = "string", Description = "The name, code, or ID of the family to search for." }
                    },
                    Required = new List<string> { "query" }
                }
            },
            new AiToolDefinition
            {
                Name = "get_family_details",
                Description = "Retrieve detailed information about a specific family by its unique ID.",
                Parameters = new AiToolParameters
                {
                    Properties = new Dictionary<string, AiToolParameterProperty>
                    {
                        ["id"] = new AiToolParameterProperty { Type = "string", Description = "The unique ID (GUID) of the family." }
                    },
                    Required = new List<string> { "id" }
                }
            },
            new AiToolDefinition
            {
                Name = "search_members",
                Description = "Search for family members by name, role, or other criteria.",
                Parameters = new AiToolParameters
                {
                    Properties = new Dictionary<string, AiToolParameterProperty>
                    {
                        ["query"] = new AiToolParameterProperty { Type = "string", Description = "The name, role, or other criteria to search for." },
                        ["familyId"] = new AiToolParameterProperty { Type = "string", Description = "Optional: The unique ID (GUID) of the family to filter members by." }
                    },
                    Required = new List<string> { "query" }
                }
            },
            new AiToolDefinition
            {
                Name = "get_member_details",
                Description = "Retrieve detailed information about a specific family member by their unique ID.",
                Parameters = new AiToolParameters
                {
                    Properties = new Dictionary<string, AiToolParameterProperty>
                    {
                        ["id"] = new AiToolParameterProperty { Type = "string", Description = "The unique ID (GUID) of the member." }
                    },
                    Required = new List<string> { "id" }
                }
            },
            new AiToolDefinition
            {
                Name = "search_events",
                Description = "Search for family events by name, type, date range, or family ID.",
                Parameters = new AiToolParameters
                {
                    Properties = new Dictionary<string, AiToolParameterProperty>
                    {
                        ["query"] = new AiToolParameterProperty { Type = "string", Description = "The name, type, or other criteria to search for." },
                        ["familyId"] = new AiToolParameterProperty { Type = "string", Description = "Optional: The unique ID (GUID) of the family to filter events by." },
                        ["startDate"] = new AiToolParameterProperty { Type = "string", Description = "Optional: The start date for the event search (YYYY-MM-DD format)." },
                        ["endDate"] = new AiToolParameterProperty { Type = "string", Description = "Optional: The end date for the event search (YYYY-MM-DD format)." }
                    },
                    Required = new List<string> { "query" }
                }
            },
            new AiToolDefinition
            {
                Name = "get_upcoming_events",
                Description = "Retrieve a list of upcoming family events for a specific family.",
                Parameters = new AiToolParameters
                {
                    Properties = new Dictionary<string, AiToolParameterProperty>
                    {
                        ["familyId"] = new AiToolParameterProperty { Type = "string", Description = "Optional: The unique ID (GUID) of the family to filter upcoming events by." }
                    },
                    Required = new List<string> { }
                }
            }
        };
    }

    public async Task<string> GetStatusAsync(string? providerName = null)
    {
        var selectedProviderName = providerName ?? _defaultAiProvider;
        IAiProvider aiProvider;
        try
        {
            aiProvider = _aiProviderFactory.GetProvider(selectedProviderName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid AI provider specified for status check: {ProviderName}", selectedProviderName);
            return $"Error: Invalid AI provider '{selectedProviderName}'.";
        }

        return await aiProvider.GetStatusAsync();
    }
}
