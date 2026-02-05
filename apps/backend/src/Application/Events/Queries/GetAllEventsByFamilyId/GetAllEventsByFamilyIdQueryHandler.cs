using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Events.Queries.GetAllEventsByFamilyId;

public class GetAllEventsByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<GetAllEventsByFamilyIdQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<List<EventDto>>> Handle(GetAllEventsByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Include(e => e.EventMembers).ThenInclude(em => em.Member)
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
