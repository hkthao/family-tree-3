namespace backend.Domain.Entities
{
    public class TextChunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = null!;
        public Guid FamilyId { get; set; }
        public string Category { get; set; } = null!;
        public string? Source { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = [];
        public double[]? Embedding { get; set; }
        public float Score { get; set; }
    }
}
