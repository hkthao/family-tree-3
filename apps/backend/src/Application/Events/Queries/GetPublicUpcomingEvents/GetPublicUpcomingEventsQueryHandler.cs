using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;
using backend.Application.Events.Queries; // Add this for EventDto

namespace backend.Application.Events.Queries.GetPublicUpcomingEvents;

public class GetPublicUpcomingEventsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPublicUpcomingEventsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<EventDto>>> Handle(GetPublicUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        var spec = new PublicEventsSpecification();
        var query = _context.Events.AsNoTracking().WithSpecification(spec);

        if (request.FamilyId.HasValue)
        {
            query = query.WithSpecification(new EventsByFamilyIdSpecification(request.FamilyId.Value));
        }

        // Removed date range and order by start date specifications as they are no longer valid.
        // The logic for "upcoming" will be part of the new EventOccurrenceService.

        var events = await query
            .OrderBy(e => e.Name) // Placeholder: Order by name for now
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(events);
    }
}
