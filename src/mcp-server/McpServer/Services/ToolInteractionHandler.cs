using System.Text.Json;

namespace McpServer.Services;

/// <summary>
/// Xử lý các tương tác AI liên quan đến việc gọi và thực thi công cụ.
/// </summary>
public class ToolInteractionHandler
{
    private readonly IAiProvider _aiProvider;
    private readonly ToolExecutor _toolExecutor;
    private readonly ILogger<ToolInteractionHandler> _logger;

    public ToolInteractionHandler(IAiProvider aiProvider, ToolExecutor toolExecutor, ILogger<ToolInteractionHandler> logger)
    {
        _aiProvider = aiProvider;
        _toolExecutor = toolExecutor;
        _logger = logger;
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
    public virtual async IAsyncEnumerable<string> HandleToolInteractionAsync(string prompt, string? jwtToken)
    {
        List<string> finalResponseChunks = [];
        var tools = DefineTools();

        // Gửi prompt và danh sách tool đến LLM
        _logger.LogInformation("Phase 1: Sending prompt and tool definitions to LLM.");
        var responseParts = _aiProvider.GenerateToolUseResponseStreamAsync(prompt, tools);

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

        // Construct a new prompt for the LLM to generate a natural language response based on tool results
        var toolResultsJson = JsonSerializer.Serialize(toolResults);
        var followUpPrompt = $"Dựa trên yêu cầu ban đầu của người dùng: '{prompt}' và các kết quả từ việc thực thi công cụ: {toolResultsJson}, hãy tạo một phản hồi tự nhiên và hữu ích cho người dùng.";

        _logger.LogInformation("followUpPrompt: {followUpPrompt}", followUpPrompt);

        var finalResponseParts = _aiProvider.GenerateChatResponseStreamAsync(followUpPrompt);

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
                    _logger.LogWarning("LLM attempted to call tools in the final response phase. Executing them and re-prompting.");
                    // Execute these unexpected tool calls
                    foreach (var unexpectedToolCall in toolCallPart.ToolCalls)
                    {
                        var toolResult = await _toolExecutor.ExecuteToolCallAsync(unexpectedToolCall, jwtToken);
                        toolResults.Add(toolResult);
                    }

                    // Re-prompt the LLM with the new tool results
                    int rePromptAttempt = 0;
                    const int maxRePromptAttempts = 2; // Allow 2 additional re-prompts

                    bool textResponseReceived = false;
                    while (rePromptAttempt < maxRePromptAttempts && !textResponseReceived)
                    {
                        var rePromptResponseParts = _aiProvider.GenerateChatResponseStreamAsync(followUpPrompt);
                        await foreach (var rePromptPart in rePromptResponseParts)
                        {
                            if (rePromptPart is AiTextResponsePart rePromptTextPart)
                            {
                                finalResponseChunks.Add(rePromptTextPart.Text);
                                textResponseReceived = true;
                                break; // Exit inner foreach
                            }
                            else if (rePromptPart is AiToolCallResponsePart rePromptToolCallPart)
                            {
                                if (rePromptToolCallPart.ToolCalls.Any())
                                {
                                    _logger.LogWarning("LLM still attempted to call tools after re-prompt attempt {Attempt}. Executing them and re-prompting again.", rePromptAttempt + 1);
                                    foreach (var furtherUnexpectedToolCall in rePromptToolCallPart.ToolCalls)
                                    {
                                        var toolResult = await _toolExecutor.ExecuteToolCallAsync(furtherUnexpectedToolCall, jwtToken);
                                        toolResults.Add(toolResult);
                                    }
                                    rePromptAttempt++;
                                    // Continue to next iteration of while loop to re-prompt
                                    break; // Exit inner foreach
                                }
                            }
                        }

                        if (!textResponseReceived && rePromptAttempt >= maxRePromptAttempts)
                        {
                            _logger.LogError("LLM persistently attempted to call tools after {MaxAttempts} re-prompts. Returning a fallback error.", maxRePromptAttempts);
                            finalResponseChunks.Add("Error: The AI is unable to provide a definitive answer at this time as it's repeatedly trying to use tools. Please try rephrasing your request or contact support.");
                        }
                    }
                }
            }
        }
        _logger.LogInformation("Finished streaming final response from LLM.");
        foreach (var chunk in finalResponseChunks) yield return chunk;
    }
}
