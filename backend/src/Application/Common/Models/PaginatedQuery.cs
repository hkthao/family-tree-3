namespace backend.Application.Common.Models;

public abstract record PaginatedQuery
{
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}
