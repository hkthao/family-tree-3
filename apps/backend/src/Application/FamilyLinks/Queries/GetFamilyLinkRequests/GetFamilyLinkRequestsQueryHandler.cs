using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinkRequests;

public class GetFamilyLinkRequestsQueryHandler : IRequestHandler<GetFamilyLinkRequestsQuery, Result<List<FamilyLinkRequestDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public GetFamilyLinkRequestsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FamilyLinkRequestDto>>> Handle(GetFamilyLinkRequestsQuery request, CancellationToken cancellationToken)
    {
        // 1. Authorization: Only admin of the family can view its link requests
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<List<FamilyLinkRequestDto>>.Forbidden("Bạn không có quyền xem các yêu cầu liên kết của gia đình này.");
        }

        var requests = await _context.FamilyLinkRequests
            .Include(lr => lr.RequestingFamily)
            .Include(lr => lr.TargetFamily)
            .Where(lr => lr.RequestingFamilyId == request.FamilyId || lr.TargetFamilyId == request.FamilyId)
            .ProjectTo<FamilyLinkRequestDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<FamilyLinkRequestDto>>.Success(requests);
    }
}
