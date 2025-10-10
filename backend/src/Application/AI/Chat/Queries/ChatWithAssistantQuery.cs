using MediatR;
using backend.Application.AI.Chat;

namespace backend.Application.AI.Chat.Queries;

public record ChatWithAssistantQuery(string UserMessage, string? SessionId = null) : IRequest<ChatResponse>;
