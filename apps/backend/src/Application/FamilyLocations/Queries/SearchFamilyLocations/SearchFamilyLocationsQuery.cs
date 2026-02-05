using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Extensions;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Specifications;
using backend.Domain.Enums;

namespace backend.Application.FamilyLocations.Queries.SearchFamilyLocations;

public record SearchFamilyLocationsQuery : PaginatedQuery, IRequest<Result<PaginatedList<FamilyLocationDto>>>
{
    public Guid? FamilyId { get; init; }
    public string? SearchQuery { get; init; }
    public LocationType? LocationType { get; init; }
    public LocationSource? Source { get; init; }
}

public class SearchFamilyLocationsQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUser, IAuthorizationService authorizationService, IPrivacyService privacyService) : IRequestHandler<SearchFamilyLocationsQuery, Result<PaginatedList<FamilyLocationDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<PaginatedList<FamilyLocationDto>>> Handle(SearchFamilyLocationsQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng
        if (!_currentUser.IsAuthenticated)
        {
            return Result<PaginatedList<FamilyLocationDto>>.Success(PaginatedList<FamilyLocationDto>.Empty());
        }

        var currentUserId = _currentUser.UserId;
        var isAdmin = _authorizationService.IsAdmin();

        var query = _context.FamilyLocations
            .Include(e => e.Location)
            .Include(l => l.Family) // Include Family for access control
            .ThenInclude(f => f!.FamilyUsers) // Include FamilyUsers for access control
            .AsNoTracking()
            .AsQueryable();

        // Apply access control based on whether FamilyId is provided in the request
        if (request.FamilyId.HasValue)
        {
            // If a specific FamilyId is requested, check if the user has access to it
            if (!_authorizationService.CanAccessFamily(request.FamilyId.Value))
            {
                return Result<PaginatedList<FamilyLocationDto>>.Success(PaginatedList<FamilyLocationDto>.Empty());
            }
            query = query.WithSpecification(new FamilyLocationByFamilyIdSpecification(request.FamilyId));
        }
        else
        {
            // If no specific FamilyId is requested, filter by all families the user has access to
            query = query.WithSpecification(new FamilyLocationAccessSpecification(isAdmin, currentUserId));
        }

        query = query.WithSpecification(new FamilyLocationSearchQuerySpecification(request.SearchQuery));
        query = query.WithSpecification(new FamilyLocationByLocationTypeSpecification(request.LocationType));
        query = query.WithSpecification(new FamilyLocationBySourceSpecification(request.Source));
        query = query.WithSpecification(new FamilyLocationOrderingSpecification(request.SortBy, request.SortOrder));

        var paginatedFamilyLocationEntities = await query
            .PaginatedListAsync(request.Page, request.ItemsPerPage);

        var familyLocationDtos = _mapper.Map<List<FamilyLocationDto>>(paginatedFamilyLocationEntities.Items);

        var filteredFamilyLocationDtos = new List<FamilyLocationDto>();
        foreach (var familyLocationDto in familyLocationDtos)
        {
            filteredFamilyLocationDtos.Add(await _privacyService.ApplyPrivacyFilter(familyLocationDto, familyLocationDto.FamilyId, cancellationToken));
        }

        var filteredPaginatedList = new PaginatedList<FamilyLocationDto>(
            filteredFamilyLocationDtos,
            paginatedFamilyLocationEntities.TotalItems,
            paginatedFamilyLocationEntities.Page,
            paginatedFamilyLocationEntities.TotalPages
        );

        return Result<PaginatedList<FamilyLocationDto>>.Success(filteredPaginatedList);
    }
}
