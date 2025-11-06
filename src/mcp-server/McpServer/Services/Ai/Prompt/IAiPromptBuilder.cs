using McpServer.Models;
using McpServer.Services.Ai.AITools;

namespace McpServer.Services.Ai.Prompt;

public interface IAiPromptBuilder
{
    List<AiMessage> BuildPromptForToolUse(string userPrompt, List<ToolDefinition>? tools);
    List<AiMessage> BuildPromptForChat(string userPrompt, List<McpServer.Models.AiToolResult>? toolResults);
    List<AiMessage> BuildPromptForChat(string userPrompt);
}