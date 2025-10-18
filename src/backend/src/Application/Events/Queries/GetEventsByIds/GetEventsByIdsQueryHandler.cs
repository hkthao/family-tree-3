using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandler : IRequestHandler<GetEventsByIdsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventsByIdsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<EventDto>>> Handle(GetEventsByIdsQuery request, CancellationToken cancellationToken)
    {
        var eventList = await _context.Events
            .Where(f => request.Ids.Contains(f.Id))
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(eventList);
    }
}
