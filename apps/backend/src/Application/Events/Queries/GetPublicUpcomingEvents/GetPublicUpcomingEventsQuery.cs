using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Events.Queries.GetPublicUpcomingEvents;

public record GetPublicUpcomingEventsQuery : IRequest<Result<List<EventDto>>>
{
    public Guid? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetPublicUpcomingEventsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetPublicUpcomingEventsQuery, Result<List<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<List<EventDto>>> Handle(GetPublicUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events
            .AsNoTracking()
            .Where(e => e.Family.Visibility == FamilyVisibility.Public);

        if (request.FamilyId.HasValue)
        {
            query = query.Where(e => e.FamilyId == request.FamilyId.Value);
        }

        var now = DateTime.UtcNow;
        var startDate = request.StartDate ?? now;
        var endDate = request.EndDate ?? now.AddMonths(3); // Default to next 3 months

        query = query.Where(e => e.StartDate >= startDate && e.StartDate <= endDate)
                     .OrderBy(e => e.StartDate);

        var events = await query
            .ProjectToType<EventDto>()
            .ToListAsync(cancellationToken);

        return Result<List<EventDto>>.Success(events);
    }
}

// EventDto is defined in GetPublicEventByIdQuery.cs, so we don't redefine it here.
