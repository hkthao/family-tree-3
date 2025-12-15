using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;
using backend.Application.Events.Queries;

namespace backend.Application.Events.Queries.SearchPublicEvents;

public class SearchPublicEventsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchPublicEventsQuery, Result<PaginatedList<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<EventDto>>> Handle(SearchPublicEventsQuery request, CancellationToken cancellationToken)
    {
        var spec = new PublicEventsSpecification(); // Assuming this specification is still valid
        var query = _context.Events.AsNoTracking().WithSpecification(spec);

        if (request.FamilyId.HasValue)
        {
            query = query.WithSpecification(new EventsByFamilyIdSpecification(request.FamilyId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.WithSpecification(new EventsBySearchTermSpecification(request.SearchTerm));
        }

        if (request.EventType.HasValue)
        {
            query = query.WithSpecification(new EventsByEventTypeSpecification(request.EventType.Value));
        }

        // Removed EventsByDateRangeSpecification usage as it's no longer valid

        // Sorting
        // Since StartDate is removed, we'll sort by Name or Code as a default
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "code" => request.SortOrder == "desc" ? query.OrderByDescending(e => e.Code) : query.OrderBy(e => e.Code),
            _ => query.OrderBy(e => e.Name) // Default sort if no sortBy is provided or relevant date fields are not applicable
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var events = await query
            .Include(e => e.EventMembers).ThenInclude(em => em.Member) // Include related members
            .Skip((request.Page - 1) * request.ItemsPerPage)
            .Take(request.ItemsPerPage)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<EventDto>(events, totalItems, request.Page, request.ItemsPerPage);
        return Result<PaginatedList<EventDto>>.Success(paginatedList);
    }
}

