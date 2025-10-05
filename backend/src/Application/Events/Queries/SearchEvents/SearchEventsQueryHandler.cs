using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Mappings;
using backend.Domain.Enums;

namespace backend.Application.Events.Queries.SearchEvents;

public class SearchEventsQueryHandler : IRequestHandler<SearchEventsQuery, PaginatedList<EventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchEventsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<EventDto>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            query = query.Where(e => e.Name.Contains(request.SearchQuery) || (e.Description != null && e.Description.Contains(request.SearchQuery)));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(e => e.StartDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(e => e.StartDate <= request.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            if (Enum.TryParse<EventType>(request.Type, true, out var eventType))
            {
                query = query.Where(e => e.Type == eventType);
            }
        }

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "name":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name);
                    break;
                case "startdate":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate);
                    break;
                case "created":
                    query = request.SortOrder == "desc" ? query.OrderByDescending(e => e.Created) : query.OrderBy(e => e.Created);
                    break;
                default:
                    query = query.OrderBy(e => e.StartDate); // Default sort
                    break;
            }
        }
        else
        {
            query = query.OrderBy(e => e.StartDate); // Default sort if no sortBy is provided
        }

        return await query
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);
    }
}