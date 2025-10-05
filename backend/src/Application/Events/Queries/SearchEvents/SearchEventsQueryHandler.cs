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

        // Apply individual specifications
        query = query.WithSpecification(new EventSearchTermSpecification(request.SearchQuery));
        query = query.WithSpecification(new EventDateRangeSpecification(request.StartDate, request.EndDate));
        query = query.WithSpecification(new EventTypeSpecification(request.Type));
        query = query.WithSpecification(new EventByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new EventByMemberIdSpecification(request.MemberId));

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