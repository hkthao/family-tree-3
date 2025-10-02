using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events
            .Where(e => e.Id == request.Id)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return entity ?? throw new Common.Exceptions.NotFoundException(nameof(Event), request.Id);
    }
}
