using System.Text;
using System.Text.Json;
using McpServer.Models;
using McpServer.Services.Ai.AITools; // For ToolDefinition

namespace McpServer.Services.Ai.Prompt;

public class AiPromptBuilder : IAiPromptBuilder
{
    private readonly ISystemPromptManager _systemPromptManager;
    private readonly ILogger<AiPromptBuilder> _logger; // Inject ILogger

    public AiPromptBuilder(ISystemPromptManager systemPromptManager, ILogger<AiPromptBuilder> logger) // Update constructor
    {
        _systemPromptManager = systemPromptManager;
        _logger = logger; // Assign logger
    }

    public List<AiMessage> BuildPromptForToolUse(string userPrompt, List<ToolDefinition>? tools)
    {
        var messages = new List<AiMessage>
        {
            new(AiMessageRole.System.ToString(),
                "You are an assistant. Only generate JSON for tool calls. " +
                "Do NOT provide natural language answers. " +
                "Use the schema: {\"tool_calls\": [{\"id\": \"<uuid>\", \"function\": {\"name\": \"<tool_name>\", \"arguments\": \"<json_arguments>\"}}]}")
        };

        if (tools != null && tools.Any())
        {
            // Provide tools schema for AI
            var toolsSchema = tools.Select(tool => new
            {
                name = tool.Name,
                description = tool.Description,
                parameters = tool.Parameters
            });
            messages.Add(new AiMessage(AiMessageRole.System.ToString(),
                "Available tools (with parameters schema):\n" +
                JsonSerializer.Serialize(toolsSchema, new JsonSerializerOptions { WriteIndented = true })));
        }

        // User query
        messages.Add(new AiMessage(AiMessageRole.User.ToString(), userPrompt));

        return messages;
    }

    public List<AiMessage> BuildPromptForChat(string userPrompt, List<McpServer.Models.AiToolResult>? toolResults)
    {
        var messages = new List<AiMessage>
        {
            new(AiMessageRole.System.ToString(), _systemPromptManager.GetDefaultSystemPrompt()),
            new(AiMessageRole.User.ToString(), userPrompt)
        };

        // Include previous tool results as context for AI to synthesize answer
        if (toolResults != null && toolResults.Count != 0)
        {
            var sbResults = new StringBuilder();
            sbResults.AppendLine("Previous tool call results (use them to answer the user's query):");

            foreach (var result in toolResults)
            {
                sbResults.AppendLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }

            messages.Add(new AiMessage(AiMessageRole.Assistant.ToString(), sbResults.ToString()));
        }

        _logger.LogDebug(
            "Constructed prompt for chat with tool results: {Prompt}",
            JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true })
        );

        return messages;
    }

    public List<AiMessage> BuildPromptForChat(string userPrompt)
    {
        return BuildPromptForChat(userPrompt, null);
    }
}
