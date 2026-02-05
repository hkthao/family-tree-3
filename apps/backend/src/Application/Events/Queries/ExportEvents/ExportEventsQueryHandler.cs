using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using Newtonsoft.Json;

namespace backend.Application.Events.Queries.ExportEvents;

public class ExportEventsQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<ExportEventsQuery, Result<string>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<string>> Handle(ExportEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Where(e => e.FamilyId == request.FamilyId)
            .Include(e => e.EventMembers)
            .ThenInclude(em => em.Member)
            .Include(e => e.LunarDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!events.Any())
        {
            return Result<string>.Failure("Không tìm thấy sự kiện nào cho gia đình này.");
        }

        var eventDtos = _mapper.Map<List<EventDto>>(events);
        var filteredEventDtos = new List<EventDto>();
        foreach (var eventDto in eventDtos)
        {
            // Apply privacy filter to each EventDto
            filteredEventDtos.Add(await _privacyService.ApplyPrivacyFilter(eventDto, request.FamilyId, cancellationToken));
        }

        var json = JsonConvert.SerializeObject(filteredEventDtos, Formatting.Indented);

        return Result<string>.Success(json);
    }
}
