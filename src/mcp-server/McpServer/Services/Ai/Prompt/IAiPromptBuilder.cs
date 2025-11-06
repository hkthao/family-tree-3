using McpServer.Models;
using McpServer.Services.Ai.Tools;

namespace McpServer.Services.Ai.Prompt;

public interface IAiPromptBuilder
{
    List<AiMessage> BuildPromptForToolUse(string userPrompt, List<AiToolDefinition>? tools);
    List<AiMessage> BuildPromptForChat(string userPrompt, List<AiToolResult>? toolResults);
    List<AiMessage> BuildPromptForChat(string userPrompt);
}