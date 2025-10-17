using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models; // Added
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Result<EventDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<EventDetailDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new EventByIdSpecification(request.Id);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = _context.Events.AsQueryable().WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var eventDto = await query
            .ProjectTo<EventDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (eventDto == null)
        {
            return Result<EventDetailDto>.Failure($"Event with ID {request.Id} not found.");
        }

        return Result<EventDetailDto>.Success(eventDto);
    }
}
