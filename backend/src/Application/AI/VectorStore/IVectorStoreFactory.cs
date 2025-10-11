using backend.Domain.Enums;

namespace backend.Application.AI.VectorStore;

public interface IVectorStoreFactory
{
    IVectorStore CreateVectorStore(VectorStoreProviderType provider);
}
