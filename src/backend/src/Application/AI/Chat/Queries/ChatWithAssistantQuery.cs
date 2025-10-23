using backend.Application.Common.Models;

namespace backend.Application.AI.Chat.Queries;

public record ChatWithAssistantQuery(string UserMessage, string? SessionId = null) : IRequest<Result<ChatResponse>>;
