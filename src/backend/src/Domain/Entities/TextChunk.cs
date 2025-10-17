namespace backend.Domain.Entities
{
    public class TextChunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = null!;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public float[]? Embedding { get; set; }
        public float Score { get; set; }
    }
}
