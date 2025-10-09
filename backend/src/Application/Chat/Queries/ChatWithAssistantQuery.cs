namespace backend.Application.Chat.Queries;

public record ChatWithAssistantQuery(string UserMessage, string? SessionId = null) : IRequest<ChatResponse>;
