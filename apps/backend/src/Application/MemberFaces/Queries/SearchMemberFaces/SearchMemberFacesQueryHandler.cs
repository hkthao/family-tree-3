using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore; // Add this import
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For MemberFaceDto and BoundingBoxDto
using backend.Application.MemberFaces.Specifications; // Import the new specification
using backend.Domain.Entities;

namespace backend.Application.MemberFaces.Queries.SearchMemberFaces;
public class SearchMemberFacesQueryHandler : IRequestHandler<SearchMemberFacesQuery, Result<PaginatedList<MemberFaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser
    private readonly IAuthorizationService _authorizationService; // Inject IAuthorizationService
    private readonly IMapper _mapper; // Inject IMapper

    public SearchMemberFacesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser, IAuthorizationService authorizationService, IMapper mapper) // Modify constructor
    {
        _context = context;
        _currentUser = currentUser;
        _authorizationService = authorizationService;
        _mapper = mapper; // Assign
    }
    public async Task<Result<PaginatedList<MemberFaceDto>>> Handle(SearchMemberFacesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        var isAdmin = _authorizationService.IsAdmin(); // Use IAuthorizationService.IsAdmin()

        // Apply authorization specification
        var accessSpec = new MemberFaceAccessSpecification(isAdmin, currentUserId);
        IQueryable<MemberFace> query = _context.MemberFaces.WithSpecification(accessSpec);

        // Apply search and sort specification
        var searchAndSortSpec = new SearchMemberFacesSpecification(request);
        query = query.WithSpecification(searchAndSortSpec);

        if (!await query.AnyAsync(cancellationToken))
        {
            return Result<PaginatedList<MemberFaceDto>>.Success(new PaginatedList<MemberFaceDto>(new List<MemberFaceDto>(), 0, request.Page, request.ItemsPerPage));
        }

        var paginatedList = await PaginatedList<MemberFaceDto>.CreateAsync(
            query.ProjectTo<MemberFaceDto>(_mapper.ConfigurationProvider).AsNoTracking(),
            request.Page,
            request.ItemsPerPage
        );
        return Result<PaginatedList<MemberFaceDto>>.Success(paginatedList);
    }
}
