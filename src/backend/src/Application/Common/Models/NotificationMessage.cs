namespace backend.Application.Common.Models;

public class NotificationMessage
{
    public string RecipientUserId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Dictionary<string, string>? Data { get; set; }
    public string? DeepLink { get; set; }
}
