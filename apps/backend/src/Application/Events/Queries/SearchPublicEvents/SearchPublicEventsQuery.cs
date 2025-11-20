using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Events.Queries.SearchPublicEvents;

public record SearchPublicEventsQuery : IRequest<Result<PaginatedList<EventDto>>>
{
    public string? SearchTerm { get; init; }
    public Guid? FamilyId { get; init; }
    public EventType? EventType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int ItemsPerPage { get; init; } = 10;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } // "asc" or "desc"
}

public class SearchPublicEventsQueryHandler(IApplicationDbContext context) : IRequestHandler<SearchPublicEventsQuery, Result<PaginatedList<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Result<PaginatedList<EventDto>>> Handle(SearchPublicEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events
            .AsNoTracking()
            .Where(e => e.Family.Visibility == FamilyVisibility.Public);

        if (request.FamilyId.HasValue)
        {
            query = query.Where(e => e.FamilyId == request.FamilyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(e => e.Title!.Contains(request.SearchTerm) ||
                                     e.Description!.Contains(request.SearchTerm) ||
                                     e.Location!.Contains(request.SearchTerm));
        }

        if (request.EventType.HasValue)
        {
            query = query.Where(e => e.EventType == request.EventType.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(e => e.StartDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(e => e.StartDate <= request.EndDate.Value);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortOrder == "desc" ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "startdate" => request.SortOrder == "desc" ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate),
            _ => query.OrderBy(e => e.StartDate) // Default sort
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var events = await query
            .Skip((request.Page - 1) * request.ItemsPerPage)
            .Take(request.ItemsPerPage)
            .ProjectToType<EventDto>()
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<EventDto>(events, totalItems, request.Page, request.ItemsPerPage);
        return Result<PaginatedList<EventDto>>.Success(paginatedList);
    }
}

// EventDto is defined in GetPublicEventByIdQuery.cs, so we don't redefine it here.
// If it were not, we would define it here or in a common DTO file.
