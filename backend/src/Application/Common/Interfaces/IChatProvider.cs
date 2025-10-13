using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IChatProvider
{
    Task<string> GenerateResponseAsync(List<ChatMessage> messages);
}
