using System.Text;
using System.Text.Json;

namespace McpServer.Services;

public class AiPromptBuilder : IAiPromptBuilder
{
    public string BuildPromptForToolUse(string userPrompt, List<AiToolDefinition>? tools, List<AiToolResult>? toolResults)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful assistant with access to the following tools.");
        sb.AppendLine("To use a tool, you must respond with a JSON object matching the following schema:");
        sb.AppendLine("{\"tool_calls\": [{\"id\": \"<unique_id>\", \"function\": {\"name\": \"<tool_name>\", \"arguments\": \"<json_escaped_arguments>\"}}]}");
        sb.AppendLine("If you don't need to use a tool, just respond with a regular text message.");
        sb.AppendLine("\nIMPORTANT TOOL USAGE GUIDELINES:");
        sb.AppendLine("- Use the 'search_family' tool when the user provides a family name, keyword, or any general query to find families.");
        sb.AppendLine("- ONLY use the 'get_family_details' tool when you have a specific, confirmed family ID (GUID). Do NOT use it with a family name or general query.");
        sb.AppendLine("\nHere are the available tools:");
        sb.AppendLine(JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true }));

        if (toolResults != null && toolResults.Any())
        {
            sb.AppendLine("\nYou have previously called tools and received these results:");
            sb.AppendLine(JsonSerializer.Serialize(toolResults, new JsonSerializerOptions { WriteIndented = true }));
            sb.AppendLine("\nBased on these results, please provide a final answer to the user's original query. DO NOT call any more tools.");
        }

        sb.AppendLine("\nUser Query:");
        sb.AppendLine(userPrompt);

        return sb.ToString();
    }

    public string BuildPromptForChat(string userPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful assistant. Provide a concise and natural language response to the user's query.");
        sb.AppendLine("\nUser Query:");
        sb.AppendLine(userPrompt);
        return sb.ToString();
    }
}
