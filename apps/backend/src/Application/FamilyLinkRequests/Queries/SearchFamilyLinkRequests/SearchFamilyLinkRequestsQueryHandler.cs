using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyLinks.Queries; // New using directive
using backend.Domain.Enums; // For LinkStatus

namespace backend.Application.FamilyLinkRequests.Queries.SearchFamilyLinkRequests;

public class SearchFamilyLinkRequestsQueryHandler : IRequestHandler<SearchFamilyLinkRequestsQuery, Result<PaginatedList<FamilyLinkRequestDto>>> // Updated class name and return type
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public SearchFamilyLinkRequestsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<FamilyLinkRequestDto>>> Handle(SearchFamilyLinkRequestsQuery request, CancellationToken cancellationToken) // Updated return type
    {
        // 1. Authorization: User must be a member of the family to view its links
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<PaginatedList<FamilyLinkRequestDto>>.Forbidden("Bạn không có quyền xem các yêu cầu liên kết của gia đình này.");
        }

        var requestsQuery = _context.FamilyLinkRequests
            .Include(flr => flr.RequestingFamily)
            .Include(flr => flr.TargetFamily)
            .Where(flr => flr.RequestingFamilyId == request.FamilyId || flr.TargetFamilyId == request.FamilyId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            requestsQuery = requestsQuery.Where(flr =>
                flr.RequestingFamily.Name.Contains(request.SearchQuery) ||
                flr.TargetFamily.Name.Contains(request.SearchQuery) ||
                (flr.RequestMessage != null && flr.RequestMessage.Contains(request.SearchQuery)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<LinkStatus>(request.Status, true, out var statusEnum))
            {
                requestsQuery = requestsQuery.Where(flr => flr.Status == statusEnum);
            }
        }

        if (request.OtherFamilyId.HasValue)
        {
            requestsQuery = requestsQuery.Where(flr =>
                (flr.RequestingFamilyId == request.FamilyId && flr.TargetFamilyId == request.OtherFamilyId.Value) ||
                (flr.TargetFamilyId == request.FamilyId && flr.RequestingFamilyId == request.OtherFamilyId.Value));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            requestsQuery = request.SortBy.ToLowerInvariant() switch
            {
                "requestingfamilyname" => request.SortOrder?.ToLowerInvariant() == "desc"
                    ? requestsQuery.OrderByDescending(flr => flr.RequestingFamily.Name)
                    : requestsQuery.OrderBy(flr => flr.RequestingFamily.Name),
                "targetfamilyname" => request.SortOrder?.ToLowerInvariant() == "desc"
                    ? requestsQuery.OrderByDescending(flr => flr.TargetFamily.Name)
                    : requestsQuery.OrderBy(flr => flr.TargetFamily.Name),
                "status" => request.SortOrder?.ToLowerInvariant() == "desc"
                    ? requestsQuery.OrderByDescending(flr => flr.Status)
                    : requestsQuery.OrderBy(flr => flr.Status),
                "requestdate" => request.SortOrder?.ToLowerInvariant() == "desc"
                    ? requestsQuery.OrderByDescending(flr => flr.RequestDate)
                    : requestsQuery.OrderBy(flr => flr.RequestDate),
                "responsedate" => request.SortOrder?.ToLowerInvariant() == "desc"
                    ? requestsQuery.OrderByDescending(flr => flr.ResponseDate)
                    : requestsQuery.OrderBy(flr => flr.ResponseDate),
                _ => request.SortOrder?.ToLowerInvariant() == "desc"
                    ? requestsQuery.OrderByDescending(flr => flr.RequestDate) // Default sort
                    : requestsQuery.OrderBy(flr => flr.RequestDate) // Default sort
            };
        }
        else
        {
            requestsQuery = requestsQuery.OrderBy(flr => flr.RequestDate); // Default sort
        }

        // Apply pagination and projection
        var paginatedList = await requestsQuery.ProjectTo<FamilyLinkRequestDto>(_mapper.ConfigurationProvider).PaginatedListAsync(
            request.Page,
            request.ItemsPerPage);

        return Result<PaginatedList<FamilyLinkRequestDto>>.Success(paginatedList);
    }
}
