using System.Text.Json;
using McpServer.Services.Ai.Prompt; // For IAiPromptBuilder

namespace McpServer.Services.Ai.Tools;

/// <summary>
/// Xử lý các tương tác AI liên quan đến việc gọi và thực thi công cụ.
/// </summary>
public class ToolInteractionHandler
{
    private readonly IAiProvider _aiProvider;
    private readonly ToolExecutor _toolExecutor;
    private readonly ILogger<ToolInteractionHandler> _logger;
    private readonly IAiPromptBuilder _promptBuilder;

    public ToolInteractionHandler(IAiProvider aiProvider, ToolExecutor toolExecutor, ILogger<ToolInteractionHandler> logger, IAiPromptBuilder promptBuilder)
    {
        _aiProvider = aiProvider;
        _toolExecutor = toolExecutor;
        _logger = logger;
        _promptBuilder = promptBuilder;
    }

    /// <summary>
    /// Định nghĩa các tool có sẵn cho LLM.
    /// </summary>
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

    /// <summary>
    /// Xử lý luồng tương tác AI có gọi công cụ.
    /// </summary>
    public virtual async IAsyncEnumerable<string> HandleToolInteractionAsync(string userPrompt, string? jwtToken)
    {
        List<string> finalResponseChunks = [];
        var tools = DefineTools();

        // Build the initial prompt
        var messages = _promptBuilder.BuildPromptForToolUse(userPrompt, tools);
        _logger.LogInformation("BuildPromptForToolUse: ${messages}", JsonSerializer.Serialize(messages));

        // Gửi prompt và danh sách tool đến LLM
        _logger.LogInformation("Phase 1: Sending prompt and tool definitions to LLM.");
        var responseParts = _aiProvider.GenerateToolUseResponseStreamAsync(messages);

        var toolCalls = new List<AiToolCall>();
        await foreach (var part in responseParts)
        {
            _logger.LogInformation("Received AiResponsePart of type: {PartType}", part.GetType().Name);
            if (part is AiToolCallResponsePart toolCallPart)
            {
                _logger.LogInformation("LLM requested to call {ToolCount} tools.", toolCallPart.ToolCalls.Count);
                toolCalls.AddRange(toolCallPart.ToolCalls);
            }
        }

        // Nếu không có tool nào được gọi, kết thúc
        if (toolCalls.Count == 0)
        {
            _logger.LogInformation("No tools were called by the LLM. Finishing.");
            foreach (var chunk in finalResponseChunks) yield return chunk;
            yield break;
        }

        // Thực thi các tool call
        var toolResults = new List<AiToolResult>();
        foreach (var toolCall in toolCalls)
        {
            var toolResult = await _toolExecutor.ExecuteToolCallAsync(toolCall, jwtToken);
            _logger.LogInformation("Tool execution result for {ToolName}: {ToolResultContent}", toolCall.FunctionName, toolResult.Content);
            toolResults.Add(toolResult);
        }

        // Gửi kết quả tool call lại cho LLM để tổng hợp câu trả lời
        _logger.LogInformation("Phase 2: Sending tool results back to LLM for final response.");

        // Build the prompt with tool results
        var finalMessages = _promptBuilder.BuildPromptForChat(userPrompt, toolResults);
        _logger.LogInformation("BuildPromptForToolUse: ${messages}", JsonSerializer.Serialize(messages));

        var finalResponseParts = _aiProvider.GenerateToolUseResponseStreamAsync(finalMessages);

        await foreach (var part in finalResponseParts)
        {
            if (part is AiTextResponsePart textPart)
            {
                finalResponseChunks.Add(textPart.Text);
            }
        }
        _logger.LogInformation("Finished streaming final response from LLM.");
        foreach (var chunk in finalResponseChunks) yield return chunk;
    }
}
