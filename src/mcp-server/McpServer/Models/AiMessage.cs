namespace McpServer.Models;

public enum AiMessageRole
{
    System,
    User,
    Assistant,
    Tool
}

public class AiMessage
{
    public AiMessageRole Role { get; set; }
    public string Content { get; set; }

    public AiMessage(AiMessageRole role, string content)
    {
        Role = role;
        Content = content;
    }
}
