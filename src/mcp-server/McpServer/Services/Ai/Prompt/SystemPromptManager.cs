namespace McpServer.Services.Ai.Prompt;

public class SystemPromptManager : ISystemPromptManager
{
    public string GetDefaultSystemPrompt()
    {
        return "You are a helpful family assistant that can answer questions and call tools when needed.";
    }
}
