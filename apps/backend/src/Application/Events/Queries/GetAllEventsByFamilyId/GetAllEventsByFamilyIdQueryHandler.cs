using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetAllEventsByFamilyId;

public class GetAllEventsByFamilyIdQueryHandler : IRequestHandler<GetAllEventsByFamilyIdQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPrivacyService _privacyService;

    public GetAllEventsByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService)
    {
        _context = context;
        _mapper = mapper;
        _privacyService = privacyService;
    }

    public async Task<Result<List<EventDto>>> Handle(GetAllEventsByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Where(e => e.FamilyId == request.FamilyId)
            .OrderBy(e => e.SolarDate)
            .ToListAsync(cancellationToken);

        var eventDtos = _mapper.Map<List<Event>, List<EventDto>>(events);

        var filteredEventDtos = new List<EventDto>();
        foreach (var eventDto in eventDtos)
        {
            // Apply privacy filter to each EventDto
            filteredEventDtos.Add(await _privacyService.ApplyPrivacyFilter(eventDto, request.FamilyId, cancellationToken));
        }

        return Result<List<EventDto>>.Success(filteredEventDtos);
    }
}
