using backend.Application.Common.Models;
using backend.Application.Users.Queries;

namespace backend.Application.Users.Queries;

public record SearchUsersQuery : PaginatedQuery, IRequest<Result<PaginatedList<UserDto>>>
{
    public string? SearchQuery { get; init; }
}
