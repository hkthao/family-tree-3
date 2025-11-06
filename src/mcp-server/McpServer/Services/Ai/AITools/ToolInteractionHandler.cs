using System.Text.Json;
using McpServer.Models;

namespace McpServer.Services.Ai.AITools;

/// <summary>
/// Handles the interaction flow for AI tool calls, including execution and response generation.
/// </summary>
public class ToolInteractionHandler
{
    private readonly IAiProvider _aiProvider;
    private readonly ToolExecutor _toolExecutor;
    private readonly ToolRegistry _toolRegistry;
    private readonly ILogger<ToolInteractionHandler> _logger;

    public ToolInteractionHandler(
        IAiProvider aiProvider,
        ToolExecutor toolExecutor,
        ToolRegistry toolRegistry,
        ILogger<ToolInteractionHandler> logger)
    {
        _aiProvider = aiProvider;
        _toolExecutor = toolExecutor;
        _toolRegistry = toolRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AI interaction, potentially involving tool calls and subsequent natural language responses.
    /// </summary>
    /// <param name="userPrompt">The initial prompt from the user.</param>
    /// <param name="jwtToken">The JWT token for authorization.</param>
    /// <returns>An async enumerable of string chunks representing the AI's response.</returns>
    public virtual async IAsyncEnumerable<string> HandleToolInteractionAsync(string userPrompt, string? jwtToken)
    {
        var messages = new List<AiMessage>
        {
            new AiMessage { Role = "user", Content = userPrompt }
        };

        // Step 1: LLM suggests tool calls
        _logger.LogInformation("Step 1: Requesting tool suggestions from AI for prompt: {UserPrompt}", userPrompt);
        var toolSuggestionResponse = _aiProvider.GenerateToolUseResponseStreamAsync(messages, _toolRegistry);

        List<AiToolCall> toolCalls = new List<AiToolCall>();
        await foreach (var part in toolSuggestionResponse)
        {
            if (part.Type == "tool_call" && part.ToolCall != null)
            {
                toolCalls.Add(part.ToolCall);
            }
            // If the AI directly responds with text, yield it immediately.
            else if (part.Type == "text" && part.Text != null)
            {
                yield return part.Text;
                yield break; // Exit if AI provides a direct text response
            }
        }

        if (toolCalls.Any())
        {
            _logger.LogInformation("Step 2: AI suggested {ToolCallCount} tool calls.", toolCalls.Count);
            // Step 2: Execute tool calls
            foreach (var toolCall in toolCalls)
            {
                _logger.LogInformation("Executing tool: {ToolName} with arguments: {ToolArguments}", toolCall.Function.Name, toolCall.Function.Arguments);
                var toolResult = await _toolExecutor.ExecuteAsync(toolCall, jwtToken ?? string.Empty);

                // Add tool result to messages for the next AI call
                messages.Add(new AiMessage
                {
                    Role = "tool",
                    Content = JsonSerializer.Serialize(toolResult.Output),
                    ToolCallId = toolResult.ToolCallId
                });

                if (!toolResult.IsSuccess)
                {
                    _logger.LogError("Tool execution failed for {ToolName}: {ErrorMessage}", toolResult.ToolName, toolResult.ErrorMessage);
                    yield return $"Error executing tool {toolResult.ToolName}: {toolResult.ErrorMessage}";
                    yield break; // Stop if a tool fails
                }
            }

            // Step 3: LLM generates natural language response based on tool results
            _logger.LogInformation("Step 3: Requesting natural language response from AI after tool execution.");
            var chatResponse = _aiProvider.GenerateChatResponseStreamAsync(userPrompt); // Pass original prompt for context

            await foreach (var part in chatResponse)
            {
                if (part.Type == "text" && part.Text != null)
                {
                    yield return part.Text;
                }
            }
        }
        else
        {
            // If no tool calls were suggested and no direct text response was given initially,
            // it means the AI couldn't find a relevant tool or generate a direct response.
            _logger.LogWarning("AI did not suggest any tools and did not provide a direct text response for prompt: {UserPrompt}", userPrompt);
            yield return "I couldn't find a relevant tool or generate a direct response for your request.";
        }
    }
}