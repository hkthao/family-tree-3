using backend.Application.AI.Enums;

namespace backend.Application.AI.DTOs;

public class ChatRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string ChatInput { get; set; } = string.Empty;
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public ContextType Context { get; set; } = ContextType.Unknown; // Add new Context property
    public string? SystemPrompt { get; set; } // Add new SystemPrompt property
    public string? CollectionName { get; set; } // Add new CollectionName property
}
