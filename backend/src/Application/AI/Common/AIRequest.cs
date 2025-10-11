using backend.Domain.Enums;

namespace backend.Application.AI.Common
{
    /// <summary>
    /// Represents a request to an AI content generation service.
    /// </summary>
    public class AIRequest
    {
        public string UserPrompt { get; set; } = null!;
        public BiographyStyle Style { get; set; }
        public string Language { get; set; } = "Vietnamese";
        public int MaxTokens { get; set; } = 500;
        public bool GeneratedFromDB { get; set; } = false;
        public Guid MemberId { get; set; }
    }
}
