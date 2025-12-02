using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs;

namespace backend.Application.MemberStories.Queries.SearchStories;

public record SearchStoriesQuery : PaginatedQuery, IRequest<Result<PaginatedList<MemberStoryDto>>>
{
    public Guid? MemberId { get; init; }
    public string? SearchQuery { get; init; }
}
