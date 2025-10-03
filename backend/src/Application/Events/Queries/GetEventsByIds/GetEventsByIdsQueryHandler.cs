using backend.Application.Common.Interfaces;

namespace backend.Application.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandler : IRequestHandler<GetEventsByIdsQuery, List<EventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventsByIdsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EventDto>> Handle(GetEventsByIdsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Events
            .Where(f => request.Ids.Contains(f.Id))
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
