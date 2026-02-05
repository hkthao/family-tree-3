using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinkById;

public class GetFamilyLinkByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService) : IRequestHandler<GetFamilyLinkByIdQuery, Result<FamilyLinkDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

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
