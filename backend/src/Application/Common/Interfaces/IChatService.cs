using backend.Application.AI.Chat;

namespace backend.Application.Common.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponse> SendMessageAsync(string userMessage, string? sessionId = null);
        Task<ChatResponse> SendMessageWithContextAsync(string userMessage, IEnumerable<string> context, string? sessionId = null);
    }
}
