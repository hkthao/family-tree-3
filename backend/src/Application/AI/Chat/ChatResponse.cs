namespace backend.Application.AI.Chat;

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public List<string> Context { get; set; } = new List<string>();
}
