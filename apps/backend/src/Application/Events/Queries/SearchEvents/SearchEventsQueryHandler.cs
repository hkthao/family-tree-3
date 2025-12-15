using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Specifications;

namespace backend.Application.Events.Queries.SearchEvents;

public class SearchEventsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUserService) : IRequestHandler<SearchEventsQuery, Result<PaginatedList<EventDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUserService = currentUserService;

    public async Task<Result<PaginatedList<EventDto>>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events.AsQueryable();

        // Apply access control specification first
        var isAdmin = _authorizationService.IsAdmin();
        var currentUserId = _currentUserService.UserId;
        if (!_currentUserService.IsAuthenticated || currentUserId == Guid.Empty)
        {
            return Result<PaginatedList<EventDto>>.Success(PaginatedList<EventDto>.Empty());
        }
        query = query.WithSpecification(new EventAccessSpecification(isAdmin, currentUserId));

        // Apply individual specifications
        query = query.WithSpecification(new EventSearchTermSpecification(request.SearchQuery));
        // Removed EventDateRangeSpecification usage as it's no longer valid
        query = query.WithSpecification(new EventTypeSpecification(request.Type));
        query = query.WithSpecification(new EventByFamilyIdSpecification(request.FamilyId));
        query = query.WithSpecification(new EventByMemberIdSpecification(request.MemberId));

        // Sorting
        // Since StartDate is removed, we'll sort by Name or Code as a default
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder == "desc" ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "code" => request.SortOrder == "desc" ? query.OrderByDescending(e => e.Code) : query.OrderBy(e => e.Code),
            _ => query.OrderBy(e => e.Name) // Default sort if no sortBy is provided or relevant date fields are not applicable
        };

        var paginatedList = await query
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        return Result<PaginatedList<EventDto>>.Success(paginatedList);
    }
}
