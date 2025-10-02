using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Search.Queries.Search;

public class SearchQueryHandler : IRequestHandler<SearchQuery, PaginatedList<SearchItem>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SearchItem>> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        var familyQuery = _context.Families
            .Where(f => f.Name.Contains(request.Keyword) || (f.Description != null && f.Description.Contains(request.Keyword)))
            .Select(f => new SearchItem { Id = f.Id, Type = "Family", Name = f.Name, Description = f.Description });

        var memberQuery = _context.Members
            .Where(m => m.FirstName.Contains(request.Keyword) || m.LastName.Contains(request.Keyword) || (m.Biography != null && m.Biography.Contains(request.Keyword)))
            .Select(m => new SearchItem { Id = m.Id, Type = "Member", Name = m.FullName, Description = m.Biography });

        var eventQuery = _context.Events
            .Where(e => e.Name.Contains(request.Keyword) || (e.Description != null && e.Description.Contains(request.Keyword)))
            .Select(e => new SearchItem { Id = e.Id, Type = "Event", Name = e.Name, Description = e.Description });

        var combinedQuery = familyQuery.Union(memberQuery).Union(eventQuery);

        return await PaginatedList<SearchItem>.CreateAsync(combinedQuery.AsNoTracking(), request.PageNumber, request.PageSize);
    }
}
