namespace backend.Application.AI.VectorStore
{
    public interface IVectorStoreFactory
    {
        IVectorStore CreateVectorStore();
    }
}
