using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;

    public GetEventByIdQueryHandler(IEventRepository eventRepository, IMapper mapper)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = (await _eventRepository.GetAllAsync())
            .AsQueryable()
            .Where(e => e.Id == request.Id)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .FirstOrDefault();

        return entity ?? throw new Common.Exceptions.NotFoundException(nameof(Event), request.Id);
    }
}
