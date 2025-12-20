using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Events.Queries.GetAllEventsByFamilyId;

public class GetAllEventsByFamilyIdQueryHandler : IRequestHandler<GetAllEventsByFamilyIdQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllEventsByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<EventDto>>> Handle(GetAllEventsByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Where(e => e.FamilyId == request.FamilyId)
            .OrderBy(e => e.SolarDate)
            .ToListAsync(cancellationToken);

        var eventDtos = _mapper.Map<List<Event>, List<EventDto>>(events);

        return Result<List<EventDto>>.Success(eventDtos);
    }
}
