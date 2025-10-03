using backend.Application.Common.Interfaces;
using backend.Application.Common.Specifications;
using backend.Application.Events.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetEvents;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, IReadOnlyList<EventListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEventsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EventListDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var spec = new EventFilterSpecification(
            request.SearchTerm,
            request.EventType,
            request.FamilyId,
            request.StartDate,
            request.EndDate,
            request.Location,
            request.RelatedMemberId,
            (request.PageNumber - 1) * request.PageSize,
            request.PageSize);

        // Comment: Specification pattern is applied here to filter, sort, and page the results at the database level.
        var query = SpecificationEvaluator<Event>.GetQuery(_context.Events.AsQueryable(), spec);

        // Comment: DTO projection is used here to select only the necessary columns from the database,
        // optimizing the SQL query and reducing the amount of data transferred.
        return await query
            .ProjectTo<EventListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}