using backend.Application.Common.Models;

namespace backend.Application.AI.Queries.GetAIProviders;

/// <summary>
/// Query to retrieve a list of available AI providers and their current usage status.
/// </summary>
public record GetAIProvidersQuery : IRequest<Result<List<AIProviderDto>>>;
