using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Models.AISettings
{
    public class LocalAISettings : ILocalChatProviderSettings
    {
        public string Endpoint { get; set; } = null!;
    }
}
