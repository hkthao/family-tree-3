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
    public string Role { get; set; }
    public string Content { get; set; }

    public AiMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}
