using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs;
using backend.Domain.Entities;

namespace backend.Application.MemberStories.Queries.SearchStories;

public record SearchStoriesQuery : PaginatedQuery, IRequest<Result<PaginatedList<MemberStoryDto>>>
{
    public Guid? MemberId { get; init; }
    public string? SearchQuery { get; init; }
}

public class SearchStoriesQueryHandler : IRequestHandler<SearchStoriesQuery, Result<PaginatedList<MemberStoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchStoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<MemberStoryDto>>> Handle(SearchStoriesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<MemberStory> query = _context.MemberStories.Include(ms => ms.Member);

        if (request.MemberId.HasValue)
        {
            query = query.Where(ms => ms.MemberId == request.MemberId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.Where(ms =>
                (ms.Title != null && ms.Title.Contains(request.SearchQuery)) ||
                (ms.Story != null && ms.Story.Contains(request.SearchQuery)));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sortOrder = request.SortOrder?.ToLower();
            if (request.SortBy.Equals("MemberName", StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder == "desc")
                {
                    query = query.OrderByDescending(ms => ms.Member.LastName)
                                 .ThenByDescending(ms => ms.Member.FirstName);
                }
                else
                {
                    query = query.OrderBy(ms => ms.Member.LastName)
                                 .ThenBy(ms => ms.Member.FirstName);
                }
            }
            else if (request.SortBy.Equals("FamilyName", StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder == "desc")
                {
                    query = query.OrderByDescending(ms => ms.Member.Family.Name);
                }
                else
                {
                    query = query.OrderBy(ms => ms.Member.Family.Name);
                }
            }
            else
            {
                query = query.OrderByPropertyName(request.SortBy, sortOrder == "desc");
            }
        }
        else
        {
            // Default sorting if not specified
            query = query.OrderByDescending(ms => ms.Created);
        }

        var paginatedList = await query
            .ProjectTo<MemberStoryDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<MemberStoryDto>>.Success(paginatedList);
    }
}
