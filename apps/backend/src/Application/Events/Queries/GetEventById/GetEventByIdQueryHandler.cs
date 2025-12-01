using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetEventById;

public class GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetEventByIdQuery, Result<EventDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<EventDetailDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new EventByIdSpecification(request.Id);

        // Comment: Specification pattern is applied here to filter the result by ID at the database level.
        var query = _context.Events.AsQueryable()
            .Include(e => e.Family)
            .WithSpecification(spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        var eventDto = await query
            .ProjectTo<EventDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return eventDto == null
            ? Result<EventDetailDto>.Failure(string.Format(ErrorMessages.EventNotFound, request.Id), ErrorSources.NotFound)
            : Result<EventDetailDto>.Success(eventDto);
    }
}
