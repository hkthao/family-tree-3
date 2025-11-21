using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetPublicUpcomingEvents;

public record GetPublicUpcomingEventsQuery : IRequest<Result<List<EventDto>>>
{
    public Guid? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

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

        var now = DateTime.UtcNow;
        var startDate = request.StartDate ?? now;
        var endDate = request.EndDate ?? now.AddMonths(3); // Default to next 3 months

        query = query.WithSpecification(new EventsByDateRangeSpecification(startDate, endDate));
        query = query.WithSpecification(new EventsOrderByStartDateSpecification("asc"));

        var events = await query
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(events);
    }
}

// EventDto is defined in GetPublicEventByIdQuery.cs, so we don't redefine it here.
