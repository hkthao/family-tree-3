using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Specifications;
using backend.Application.Events.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EventDetailDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new EventByIdSpecification(request.Id);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = SpecificationEvaluator<Event>.GetQuery(_context.Events.AsQueryable(), spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var eventDto = await query
            .ProjectTo<EventDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (eventDto == null)
        {
            throw new NotFoundException(nameof(Event), request.Id);
        }

        return eventDto;
    }
}