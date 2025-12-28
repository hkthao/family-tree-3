using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Queries.ExportMemberFaces;

public class ExportMemberFacesQueryHandler : IRequestHandler<ExportMemberFacesQuery, Result<List<MemberFaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ExportMemberFacesQueryHandler> _logger;
    private readonly IMapper _mapper;

    public ExportMemberFacesQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ExportMemberFacesQueryHandler> logger, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<MemberFaceDto>>> Handle(ExportMemberFacesQuery request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin hoặc quản lý gia đình mới có thể xuất khuôn mặt thành viên
        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(request.FamilyId))
        {
            _logger.LogWarning("Người dùng không có quyền cố gắng xuất khuôn mặt thành viên cho FamilyId {FamilyId}.", request.FamilyId);
            return Result<List<MemberFaceDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Tải các MemberFace cùng với Member và Family để có thông tin đầy đủ cho MemberFaceDto
        var memberFaces = await _context.MemberFaces
            .Where(mf => mf.Member != null && mf.Member.FamilyId == request.FamilyId)
            .Include(mf => mf.Member)
                .ThenInclude(m => m!.Family)
            .AsNoTracking()
            .ProjectTo<MemberFaceDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MemberFaceDto>>.Success(memberFaces);
    }
}
