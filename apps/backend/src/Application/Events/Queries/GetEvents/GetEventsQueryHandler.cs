using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEvents;

public class GetEventsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetEventsQuery, Result<IReadOnlyList<EventListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IReadOnlyList<EventListDto>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events.AsQueryable();

        // Apply individual specifications
        if (request.RelatedMemberId.HasValue)
        {
            query = query.Include(e => e.EventMembers);
        }
        query = query.WithSpecification(new EventSearchTermSpecification(request.SearchTerm));
        query = query.WithSpecification(new EventDateRangeSpecification(request.StartDate, request.EndDate));
        query = query.WithSpecification(new EventTypeSpecification(request.EventType?.ToString())); // Convert EventType enum to string
        query = query.WithSpecification(new EventByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new EventByMemberIdSpecification(request.RelatedMemberId)); // Use RelatedMemberId for member filter
        query = query.WithSpecification(new EventByLocationSpecification(request.Location));

        // Note: GetEventsQuery does not have explicit sorting or pagination parameters beyond what PaginatedQuery provides.
        // If sorting is needed, a separate EventOrderingSpecification would be applied here.
        // Pagination is handled by the PaginatedListAsync extension method.

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var eventList = await query
            .ProjectTo<EventListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EventListDto>>.Success(eventList);
    }
}
