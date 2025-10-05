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

        // Apply ordering specification
        query = query.WithSpecification(new EventOrderingSpecification(request.SortBy, request.SortOrder));

        return await query
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);
    }
}