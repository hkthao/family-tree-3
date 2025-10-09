namespace backend.Infrastructure.Chat;

public class ChatSettings
{
    public string Provider { get; set; } = "Gemini";
    public Dictionary<string, string> ApiKeys { get; set; } = new Dictionary<string, string>();
}
