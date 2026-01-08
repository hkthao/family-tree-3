using backend.Application.Common.Models;
using backend.Application.UserPushTokens.DTOs; // Corrected namespace

namespace backend.Application.UserPushTokens.Queries.SearchUserPushTokens;

public record SearchUserPushTokensQuery : PaginatedQuery, IRequest<Result<PaginatedList<UserPushTokenDto>>>
{
    public string? SearchQuery { get; init; }
    public Guid? UserId { get; init; } // Optional: Filter by specific User ID
    public string? Platform { get; init; } // Optional: Filter by platform (android, ios, web)
    public bool? IsActive { get; init; } // Optional: Filter by active status
}
