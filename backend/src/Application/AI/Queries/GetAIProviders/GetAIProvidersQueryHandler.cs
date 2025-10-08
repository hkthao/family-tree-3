using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Application.AI.Queries.GetAIProviders;

/// <summary>
/// Handler for retrieving a list of available AI providers and their current usage status.
/// </summary>
public class GetAIProvidersQueryHandler : IRequestHandler<GetAIProvidersQuery, Result<List<AIProviderDto>>>
{
    private readonly IAIUsageTracker _aiUsageTracker;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuth0Config _auth0Config; // To get the namespace for custom claims
    private readonly IAISettings _aiSettings;

    public GetAIProvidersQueryHandler(IAIUsageTracker aiUsageTracker, IAuthorizationService authorizationService, IAuth0Config auth0Config, IAISettings aiSettings)
    {
        _aiUsageTracker = aiUsageTracker;
        _authorizationService = authorizationService;
        _auth0Config = auth0Config;
        _aiSettings = aiSettings;
    }

    public async Task<Result<List<AIProviderDto>>> Handle(GetAIProvidersQuery request, CancellationToken cancellationToken)
    {
        // Only admins can view provider details and usage
        if (!_authorizationService.IsAdmin())
        {
            return Result<List<AIProviderDto>>.Failure("Access denied. Only administrators can view AI provider details.", "Forbidden");
        }

        var providers = new List<AIProviderDto>();

        // Gemini Provider
        if (_aiSettings.Provider == AIProviderType.Gemini)
        {
            providers.Add(new AIProviderDto
            {
                ProviderType = AIProviderType.Gemini,
                Name = "Google Gemini",
                IsEnabled = true,
                DailyUsageLimit = _aiSettings.DailyUsageLimit,
                CurrentDailyUsage = (await _aiUsageTracker.GetDailyUsageCountAsync(Guid.Empty, cancellationToken)).Value, // TODO: Get actual user ID
                MaxTokensPerRequest = _aiSettings.MaxTokensPerRequest
            });
        }

        // OpenAI Provider
        if (_aiSettings.Provider == AIProviderType.OpenAI)
        {
            providers.Add(new AIProviderDto
            {
                ProviderType = AIProviderType.OpenAI,
                Name = "OpenAI",
                IsEnabled = true,
                DailyUsageLimit = _aiSettings.DailyUsageLimit,
                CurrentDailyUsage = (await _aiUsageTracker.GetDailyUsageCountAsync(Guid.Empty, cancellationToken)).Value, // TODO: Get actual user ID
                MaxTokensPerRequest = _aiSettings.MaxTokensPerRequest
            });
        }

        // LocalAI Provider
        if (_aiSettings.Provider == AIProviderType.LocalAI)
        {
            providers.Add(new AIProviderDto
            {
                ProviderType = AIProviderType.LocalAI,
                Name = "Local AI",
                IsEnabled = true,
                DailyUsageLimit = _aiSettings.DailyUsageLimit,
                CurrentDailyUsage = (await _aiUsageTracker.GetDailyUsageCountAsync(Guid.Empty, cancellationToken)).Value, // TODO: Get actual user ID
                MaxTokensPerRequest = _aiSettings.MaxTokensPerRequest
            });
        }

        return Result<List<AIProviderDto>>.Success(providers);
    }
}
