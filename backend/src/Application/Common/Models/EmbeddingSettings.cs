using backend.Application.Common.Models.AISettings;

namespace backend.Application.Common.Models
{
    public class EmbeddingSettings : AIProviderConfig
    {
        public const string SectionName = "EmbeddingSettings";
        public OpenAIEmbeddingSettings OpenAI { get; set; } = new();
        public CohereEmbeddingSettings Cohere { get; set; } = new();
        public LocalEmbeddingSettings Local { get; set; } = new();
    }

    public class OpenAIEmbeddingSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "text-embedding-ada-002";
        public int MaxTextLength { get; set; } = 8191; // Max tokens for text-embedding-ada-002
    }

    public class CohereEmbeddingSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "embed-english-v3.0";
        public int MaxTextLength { get; set; } = 512; // Max tokens for embed-english-v3.0
    }

    public class LocalEmbeddingSettings
    {
        public string ModelPath { get; set; } = string.Empty;
        public int MaxTextLength { get; set; } = 512; // Example max length for a local model
    }
}
