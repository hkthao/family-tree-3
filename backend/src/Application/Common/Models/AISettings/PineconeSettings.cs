using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Models.AISettings
{
    public class PineconeSettings : IVectorStoreProviderSettings
    {
        public string ApiKey { get; set; } = null!;
        public string Environment { get; set; } = null!;
        public string IndexName { get; set; } = null!;
    }
}
