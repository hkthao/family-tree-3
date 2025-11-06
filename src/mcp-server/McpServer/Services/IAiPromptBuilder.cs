namespace McpServer.Services;

public interface IAiPromptBuilder
{
    string BuildPromptForToolUse(string userPrompt, List<AiToolDefinition>? tools, List<AiToolResult>? toolResults);
    string BuildPromptForChat(string userPrompt);
}