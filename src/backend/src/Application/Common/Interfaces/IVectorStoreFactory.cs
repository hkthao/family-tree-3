using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

public interface IVectorStoreFactory
{
    IVectorStore CreateVectorStore(VectorStoreProviderType provider);
}
