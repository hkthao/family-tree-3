namespace backend.Application.VectorStore;

public interface IVectorStoreFactory
{
    IVectorStore CreateVectorStore();
}
