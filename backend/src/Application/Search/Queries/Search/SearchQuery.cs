using backend.Application.Common.Models;

namespace backend.Application.Search.Queries.Search;

public record SearchQuery : IRequest<PaginatedList<SearchItem>>
{
    public string Keyword { get; init; } = null!;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
