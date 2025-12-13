using backend.Application.Common.Models;

namespace backend.Application.Identity.Queries;

public record SearchUsersQuery : PaginatedQuery, IRequest<Result<PaginatedList<UserDto>>>
{
    public string? SearchQuery { get; init; }
}
