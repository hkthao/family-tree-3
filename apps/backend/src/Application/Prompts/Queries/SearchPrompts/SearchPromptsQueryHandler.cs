using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Prompts.DTOs;

namespace backend.Application.Prompts.Queries.SearchPrompts;

public class SearchPromptsQueryHandler : IRequestHandler<SearchPromptsQuery, Result<PaginatedList<PromptDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchPromptsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<PromptDto>>> Handle(SearchPromptsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Prompts.AsNoTracking(); // Use AsNoTracking for read-only operations

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            query = query.Where(p =>
                (p.Title != null && p.Title.Contains(request.SearchQuery)) ||
                (p.Content != null && p.Content.Contains(request.SearchQuery)) ||
                (p.Description != null && p.Description.Contains(request.SearchQuery)));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sortOrder = request.SortOrder?.ToLower();
            query = query.OrderByPropertyName(request.SortBy, sortOrder == "desc");
        }
        else
        {
            // Default sorting if not specified
            query = query.OrderByDescending(p => p.Created);
        }

        var paginatedList = await query
            .ProjectTo<PromptDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<PromptDto>>.Success(paginatedList);
    }
}
