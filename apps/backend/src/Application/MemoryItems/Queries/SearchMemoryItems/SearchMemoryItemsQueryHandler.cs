using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemoryItems.DTOs;

namespace backend.Application.MemoryItems.Queries.SearchMemoryItems;

public class SearchMemoryItemsQueryHandler : IRequestHandler<SearchMemoryItemsQuery, PaginatedList<MemoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPrivacyService _privacyService;

    public SearchMemoryItemsQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService)
    {
        _context = context;
        _mapper = mapper;
        _privacyService = privacyService;
    }

    public async Task<PaginatedList<MemoryItemDto>> Handle(SearchMemoryItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MemoryItems
            .Where(mi => mi.FamilyId == request.FamilyId && !mi.IsDeleted)
            .Include(mi => mi.MemoryMedia)
            .Include(mi => mi.MemoryPersons)
                .ThenInclude(mp => mp.Member)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchTermLower = request.SearchQuery.ToLower();
            query = query.Where(mi => mi.Title.ToLower().Contains(searchTermLower) ||
                                      (mi.Description != null && mi.Description.ToLower().Contains(searchTermLower)));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(mi => mi.HappenedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(mi => mi.HappenedAt <= request.EndDate.Value);
        }

        if (request.EmotionalTag.HasValue)
        {
            query = query.Where(mi => mi.EmotionalTag == request.EmotionalTag.Value);
        }

        if (request.MemberId.HasValue)
        {
            query = query.Where(mi => mi.MemoryPersons.Any(mp => mp.MemberId == request.MemberId.Value));
        }

        if (request.SortBy == "happenedAtAsc")
        {
            query = query.OrderBy(mi => mi.HappenedAt);
        }
        else if (request.SortBy == "titleAsc")
        {
            query = query.OrderBy(mi => mi.Title);
        }
        else if (request.SortBy == "titleDesc")
        {
            query = query.OrderByDescending(mi => mi.Title);
        }
        else
        {
            query = query.OrderByDescending(mi => mi.HappenedAt); // Default order
        }

        var paginatedMemoryItemEntities = await query
            .PaginatedListAsync(request.Page, request.ItemsPerPage, cancellationToken);

        var memoryItemDtos = _mapper.Map<List<MemoryItemDto>>(paginatedMemoryItemEntities.Items);

        var filteredMemoryItemDtos = new List<MemoryItemDto>();
        foreach (var memoryItemDto in memoryItemDtos)
        {
            filteredMemoryItemDtos.Add(await _privacyService.ApplyPrivacyFilter(memoryItemDto, request.FamilyId, cancellationToken));
        }

        var filteredPaginatedList = new PaginatedList<MemoryItemDto>(
            filteredMemoryItemDtos,
            paginatedMemoryItemEntities.TotalItems,
            paginatedMemoryItemEntities.Page,
            paginatedMemoryItemEntities.TotalPages
        );

        return filteredPaginatedList;
    }
}
