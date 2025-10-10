using System.Collections.Concurrent;
using backend.Application.AI.ContentGenerators;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI;

public class AIUsageTracker : IAIUsageTracker
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AIUsageTracker> _logger;
    private readonly AIContentGeneratorSettings _aiContentGeneratorSettings;

    // In-memory storage for daily usage counts and token counts
    private static readonly ConcurrentDictionary<Guid, int> _dailyUsageCounts = new();
    private static readonly ConcurrentDictionary<Guid, int> _dailyTokenCounts = new();

    public AIUsageTracker(IMemoryCache memoryCache, ILogger<AIUsageTracker> logger, IOptions<AIContentGeneratorSettings> aiContentGeneratorSettings)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _aiContentGeneratorSettings = aiContentGeneratorSettings.Value;
    }

    public async Task<Result<bool>> CheckAndRecordUsageAsync(Guid userProfileId, int tokensUsed, CancellationToken cancellationToken = default)
    {
        // Check daily usage limit
        var currentUsageCount = _dailyUsageCounts.GetOrAdd(userProfileId, 0);
        if (currentUsageCount >= _aiContentGeneratorSettings.DailyUsageLimit)
        {
            _logger.LogWarning("Daily usage limit reached for user {UserProfileId}.", userProfileId);
            return Result<bool>.Failure("Daily usage limit reached.", "DailyLimitExceeded");
        }

        // Check max tokens per request
        if (tokensUsed > _aiContentGeneratorSettings.MaxTokensPerRequest)
        {
            _logger.LogWarning("Max tokens per request exceeded for user {UserProfileId}. Requested: {TokensUsed}, Max: {MaxTokens}.", userProfileId, tokensUsed, _aiContentGeneratorSettings.MaxTokensPerRequest);
            return Result<bool>.Failure("Max tokens per request exceeded.", "MaxTokensExceeded");
        }

        // Record usage
        _dailyUsageCounts.AddOrUpdate(userProfileId, 1, (key, oldValue) => oldValue + 1);
        _dailyTokenCounts.AddOrUpdate(userProfileId, tokensUsed, (key, oldValue) => oldValue + tokensUsed);

        // Set expiration for daily limits (reset at midnight UTC)
        var midnightUtc = DateTime.UtcNow.Date.AddDays(1);
        _memoryCache.Set($"DailyUsageCount_{userProfileId}", _dailyUsageCounts[userProfileId], midnightUtc);
        _memoryCache.Set($"DailyTokenCount_{userProfileId}", _dailyTokenCounts[userProfileId], midnightUtc);

        _logger.LogInformation("Recorded AI usage for user {UserProfileId}. Tokens used: {TokensUsed}. Current daily usage: {CurrentUsageCount}.", userProfileId, tokensUsed, _dailyUsageCounts[userProfileId]);

        await Task.CompletedTask; // Satisfy async requirement
        return Result<bool>.Success(true);
    }

    public Task<Result<int>> GetRemainingDailyTokensAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var currentTokenCount = _dailyTokenCounts.GetOrAdd(userProfileId, 0);
        var remaining = _aiContentGeneratorSettings.DailyUsageLimit - currentTokenCount;
        return Task.FromResult(Result<int>.Success(remaining));
    }

    public Task<Result<int>> GetDailyUsageCountAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var currentUsageCount = _dailyUsageCounts.GetOrAdd(userProfileId, 0);
        return Task.FromResult(Result<int>.Success(currentUsageCount));
    }
}
