namespace backend.Application.AI.DTOs;

public class CallChatWebhookRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string ChatInput { get; set; } = string.Empty;
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}