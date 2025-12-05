using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinks;

public class GetFamilyLinksQueryHandler : IRequestHandler<GetFamilyLinksQuery, Result<List<FamilyLinkDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public GetFamilyLinksQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FamilyLinkDto>>> Handle(GetFamilyLinksQuery request, CancellationToken cancellationToken)
    {
        // 1. Authorization: User must be a member of the family to view its links
        if (!_authorizationService.CanAccessFamily(request.FamilyId)) // Use IsFamilyMember or IsFamilyAdmin based on desired access level
        {
            return Result<List<FamilyLinkDto>>.Forbidden("Bạn không có quyền xem các liên kết của gia đình này.");
        }

        var links = await _context.FamilyLinks
            .Include(fl => fl.Family1)
            .Include(fl => fl.Family2)
            .Where(fl => fl.Family1Id == request.FamilyId || fl.Family2Id == request.FamilyId)
            .ProjectTo<FamilyLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<FamilyLinkDto>>.Success(links);
    }
}
