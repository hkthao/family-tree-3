using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.GetPublicEventById;

public record GetPublicEventByIdQuery(Guid Id) : IRequest<Result<EventDto>>;

public class GetPublicEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPublicEventByIdQuery, Result<EventDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<EventDto>> Handle(GetPublicEventByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new PublicEventsSpecification();
        var eventEntity = await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .WithSpecification(spec)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return eventEntity == null
            ? Result<EventDto>.Failure($"Event with ID {request.Id} not found or is not public.")
            : Result<EventDto>.Success(eventEntity);
    }
}
