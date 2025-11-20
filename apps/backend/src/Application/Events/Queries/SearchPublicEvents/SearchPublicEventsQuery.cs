using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Events.Specifications;

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

public class SearchPublicEventsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<SearchPublicEventsQuery, Result<PaginatedList<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<EventDto>>> Handle(SearchPublicEventsQuery request, CancellationToken cancellationToken)
    {
        var spec = new PublicEventsSpecification();
        var query = _context.Events.AsNoTracking().WithSpecification(spec);

        if (request.FamilyId.HasValue)
        {
            query = query.WithSpecification(new EventsByFamilyIdSpecification(request.FamilyId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.WithSpecification(new EventsBySearchTermSpecification(request.SearchTerm));
        }

        if (request.EventType.HasValue)
        {
            query = query.WithSpecification(new EventsByEventTypeSpecification(request.EventType.Value));
        }

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            query = query.WithSpecification(new EventsByDateRangeSpecification(request.StartDate, request.EndDate));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => query.WithSpecification(new EventsOrderByNameSpecification(request.SortOrder ?? "asc")),
            "startdate" => query.WithSpecification(new EventsOrderByStartDateSpecification(request.SortOrder ?? "asc")),
            _ => query.WithSpecification(new EventsOrderByStartDateSpecification("asc")) // Default sort
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var events = await query
            .Skip((request.Page - 1) * request.ItemsPerPage)
            .Take(request.ItemsPerPage)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<EventDto>(events, totalItems, request.Page, request.ItemsPerPage);
        return Result<PaginatedList<EventDto>>.Success(paginatedList);
    }
}

// EventDto is defined in GetPublicEventByIdQuery.cs, so we don't redefine it here.
// If it were not, we would define it here or in a common DTO file.
