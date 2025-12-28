using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Newtonsoft.Json;

namespace backend.Application.Events.Queries.ExportEvents;

public class ExportEventsQueryHandler : IRequestHandler<ExportEventsQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ExportEventsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

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
        var json = JsonConvert.SerializeObject(eventDtos, Formatting.Indented);

        return Result<string>.Success(json);
    }
}
