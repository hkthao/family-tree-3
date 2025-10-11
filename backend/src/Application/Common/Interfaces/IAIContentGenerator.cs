using backend.Application.AI.Common;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for AI content generation services.
    /// </summary>
    public interface IAIContentGenerator
    {
        AIProviderType ProviderType { get; }
        Task<Result<AIResult>> GenerateContentAsync(AIRequest request, CancellationToken cancellationToken = default);
    }
}
