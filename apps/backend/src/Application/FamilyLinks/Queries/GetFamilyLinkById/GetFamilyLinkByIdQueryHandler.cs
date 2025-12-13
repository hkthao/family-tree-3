using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinkById;

public class GetFamilyLinkByIdQueryHandler : IRequestHandler<GetFamilyLinkByIdQuery, Result<FamilyLinkDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public GetFamilyLinkByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<FamilyLinkDto>> Handle(GetFamilyLinkByIdQuery request, CancellationToken cancellationToken)
    {
        var link = await _context.FamilyLinks
            .Include(fl => fl.Family1)
            .Include(fl => fl.Family2)
            .Where(fl => fl.Id == request.FamilyLinkId)
            .ProjectTo<FamilyLinkDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (link == null)
        {
            return Result<FamilyLinkDto>.NotFound("Không tìm thấy liên kết gia đình.");
        }

        // Authorization: User must be a member of either linked family
        if (!_authorizationService.CanAccessFamily(link.Family1Id) && !_authorizationService.CanAccessFamily(link.Family2Id))
        {
            return Result<FamilyLinkDto>.Forbidden("Bạn không có quyền xem chi tiết liên kết gia đình này.");
        }

        return Result<FamilyLinkDto>.Success(link);
    }
}
