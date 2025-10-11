namespace backend.Application.AI.Chat.Queries;

public record ChatWithAssistantQuery(string UserMessage, string? SessionId = null) : IRequest<ChatResponse>;
