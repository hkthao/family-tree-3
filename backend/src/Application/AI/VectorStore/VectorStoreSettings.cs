using backend.Application.Common.Models.AISettings;

namespace backend.Application.AI.VectorStore
{
    public class VectorStoreSettings
    {
        public const string SectionName = "VectorStoreSettings";
        public string Provider { get; set; } = "";
        public PineconeSettings Pinecone { get; set; } = null!;
    }
}
