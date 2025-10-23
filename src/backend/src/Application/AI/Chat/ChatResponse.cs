namespace backend.Application.AI.Chat;

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public List<string> Context { get; set; } = [];
    public string? SessionId { get; set; }
    public string? Model { get; set; }
    public DateTime CreatedAt { get; set; }
}
