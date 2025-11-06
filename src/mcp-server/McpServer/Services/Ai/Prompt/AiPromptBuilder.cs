using System.Text;
using System.Text.Json;
using McpServer.Models;
using McpServer.Services.Ai.Prompt; // For ISystemPromptManager
using McpServer.Services.Ai.Tools; // For AiTool related types

namespace McpServer.Services.Ai.Prompt;

public class AiPromptBuilder : IAiPromptBuilder
{
    private readonly ISystemPromptManager _systemPromptManager;

    public AiPromptBuilder(ISystemPromptManager systemPromptManager)
    {
        _systemPromptManager = systemPromptManager;
    }

    public List<AiMessage> BuildPromptForToolUse(string userPrompt, List<AiToolDefinition>? tools, List<AiToolResult>? toolResults)
    {
        var messages = new List<AiMessage>();

        messages.Add(new AiMessage(AiMessageRole.System, _systemPromptManager.GetDefaultSystemPrompt()));

        var toolInstructions = new StringBuilder();
        toolInstructions.AppendLine("You are a helpful assistant with access to the following tools.");
        toolInstructions.AppendLine("To use a tool, you must respond with a JSON object matching the following schema:");
        toolInstructions.AppendLine("{\"tool_calls\": [{\"id\": \"<unique_id>\", \"function\": {\"name\": \"<tool_name>\", \"arguments\": \"<json_escaped_arguments>\"}}]}");
        toolInstructions.AppendLine("If you don't need to use a tool, just respond with a regular text message.");
        toolInstructions.AppendLine("\nIMPORTANT TOOL USAGE GUIDELINES:");
        toolInstructions.AppendLine("- Use the 'search_family' tool when the user provides a family name, keyword, or any general query to find families.");
        toolInstructions.AppendLine("- ONLY use the 'get_family_details' tool when you have a specific, confirmed family ID (GUID). Do NOT use it with a family name or general query.");
        toolInstructions.AppendLine("\nFor example:");
        toolInstructions.AppendLine("User prompt: 'show me detail family with id 1a955fff-ce01-422f-8bb3-02ab14e8ec47'");
        toolInstructions.AppendLine("Expected response: {\"tool_calls\": [{\"id\": \"call_123\", \"function\": {\"name\": \"get_family_details\", \"arguments\": \"{\\\"id\\\": \\\"1a955fff-ce01-422f-8bb3-02ab14e8ec47\\\"}\"}}]}");
        toolInstructions.AppendLine("\nHere are the available tools:");
        toolInstructions.AppendLine(JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true }));

        messages.Add(new AiMessage(AiMessageRole.System, toolInstructions.ToString()));

        if (toolResults != null && toolResults.Any())
        {
            foreach (var toolResult in toolResults)
            {
                messages.Add(new AiMessage(AiMessageRole.Tool, JsonSerializer.Serialize(toolResult)));
            }
            messages.Add(new AiMessage(AiMessageRole.System, "Based on these results, please provide a final answer to the user's original query. DO NOT call any more tools."));
        }

        messages.Add(new AiMessage(AiMessageRole.User, userPrompt));

        return messages;
    }

    public List<AiMessage> BuildPromptForChat(string userPrompt)
    {
        var messages = new List<AiMessage>();
        messages.Add(new AiMessage(AiMessageRole.System, _systemPromptManager.GetDefaultSystemPrompt()));
        messages.Add(new AiMessage(AiMessageRole.User, userPrompt));
        return messages;
    }
}
