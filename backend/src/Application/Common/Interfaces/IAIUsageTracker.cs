using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for tracking and managing AI usage (tokens, daily limits).
    /// </summary>
    public interface IAIUsageTracker
    {
        Task<Result<bool>> CheckAndRecordUsageAsync(Guid userProfileId, int tokensUsed, CancellationToken cancellationToken = default);
        Task<Result<int>> GetRemainingDailyTokensAsync(Guid userProfileId, CancellationToken cancellationToken = default);
        Task<Result<int>> GetDailyUsageCountAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    }
}
