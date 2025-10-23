using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

public interface IChatProviderFactory
{
    IChatProvider GetProvider(ChatAIProvider provider);
}
