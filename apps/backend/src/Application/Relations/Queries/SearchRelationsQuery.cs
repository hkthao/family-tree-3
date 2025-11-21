using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Extensions; // New using statement
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Relations.Queries;

public record SearchRelationsQuery : IRequest<PaginatedList<RelationDto>>
{
    public string? Q { get; init; }
    public RelationLineage? Lineage { get; init; }
    public string? Region { get; init; } // "north", "central", "south"
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class SearchRelationsQueryHandler : IRequestHandler<SearchRelationsQuery, PaginatedList<RelationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchRelationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RelationDto>> Handle(SearchRelationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Relations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = query.Where(r => r.Name.Contains(request.Q) || r.Description.Contains(request.Q));
        }

        if (request.Lineage.HasValue)
        {
            query = query.Where(r => r.Lineage == request.Lineage.Value);
        }

        // TODO: Implement region search logic (e.g., searching within NamesByRegion JSON)
        // This will require custom JSON querying or a different approach if NamesByRegion is complex.
        // For now, it's not implemented.

        return await query
            .ProjectTo<RelationDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
