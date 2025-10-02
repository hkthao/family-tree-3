using backend.Application.Common.Interfaces;

namespace backend.Application.Events.Queries.GetEvents;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events.AsNoTracking();

        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            query = query.Where(e => e.Name.Contains(request.SearchQuery) || (e.Description != null && e.Description.Contains(request.SearchQuery)));
        }

        if (request.EventType.HasValue)
        {
            query = query.Where(e => e.Type == request.EventType.Value);
        }

        if (request.FamilyId.HasValue)
        {
            query = query.Where(e => e.FamilyId == request.FamilyId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(e => e.StartDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(e => e.EndDate <= request.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Location))
        {
            query = query.Where(e => e.Location != null && e.Location.Contains(request.Location));
        }

        if (request.RelatedMemberId.HasValue)
        {
            query = query.Where(e => e.RelatedMembers.Any(m => m.Id == request.RelatedMemberId.Value));
        }

        return await query
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
