using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEventsByIds;

public class GetEventsByIdsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetEventsByIdsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<EventDto>>> Handle(GetEventsByIdsQuery request, CancellationToken cancellationToken)
    {
        var eventsByIdsSpec = new EventsByIdsSpec(request.Ids);
        var eventList = await _context.Events
            .WithSpecification(eventsByIdsSpec)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(eventList);
    }
}
