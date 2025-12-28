using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.MemberFaces.Commands.ImportMemberFaces;

public class ImportMemberFacesCommandHandler : IRequestHandler<ImportMemberFacesCommand, Result<List<MemberFaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ImportMemberFacesCommandHandler> _logger;
    private readonly IMapper _mapper;

    public ImportMemberFacesCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ILogger<ImportMemberFacesCommandHandler> logger, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<MemberFaceDto>>> Handle(ImportMemberFacesCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra quyền: Chỉ admin hoặc quản lý gia đình mới có thể nhập khuôn mặt thành viên
        if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(request.FamilyId))
        {
            _logger.LogWarning("Người dùng không có quyền cố gắng nhập khuôn mặt thành viên cho FamilyId {FamilyId}.", request.FamilyId);
            return Result<List<MemberFaceDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Xử lý danh sách nhập rỗng một cách rõ ràng
        if (request.Faces == null || !request.Faces.Any())
        {
            return Result<List<MemberFaceDto>>.Success(new List<MemberFaceDto>());
        }

        var importedMemberFaces = new List<MemberFace>();

        foreach (var importFaceItemDto in request.Faces)
        {
            // Kiểm tra xem khuôn mặt đã tồn tại theo FaceId trong gia đình này chưa
            // Hoặc có thể kiểm tra theo MemberId và FaceId nếu MemberId được cung cấp
            var existingFace = await _context.MemberFaces
                .Include(mf => mf.Member)
                .Where(mf => mf.Member != null && mf.Member.FamilyId == request.FamilyId)
                .FirstOrDefaultAsync(mf => mf.FaceId == importFaceItemDto.FaceId, cancellationToken);

            if (existingFace != null)
            {
                _logger.LogInformation("Khuôn mặt với FaceId '{FaceId}' đã tồn tại cho FamilyId {FamilyId}. Bỏ qua nhập.", importFaceItemDto.FaceId, request.FamilyId);
                continue;
            }

            // Nếu MemberId được cung cấp, hãy kiểm tra xem Member có thuộc FamilyId này không
            if (importFaceItemDto.MemberId.HasValue)
            {
                var member = await _context.Members
                    .Where(m => m.FamilyId == request.FamilyId)
                    .FirstOrDefaultAsync(m => m.Id == importFaceItemDto.MemberId.Value, cancellationToken);

                if (member == null)
                {
                    _logger.LogWarning("Không tìm thấy MemberId '{MemberId}' trong FamilyId '{FamilyId}'. Bỏ qua nhập khuôn mặt.", importFaceItemDto.MemberId, request.FamilyId);
                    continue;
                }
            } else {
                // Nếu MemberId không được cung cấp, khuôn mặt không thể liên kết với thành viên nào
                // Có thể xử lý bằng cách bỏ qua hoặc tạo khuôn mặt không liên kết (tùy thuộc vào yêu cầu nghiệp vụ)
                _logger.LogWarning("Khuôn mặt với FaceId '{FaceId}' không có MemberId được cung cấp. Bỏ qua nhập.", importFaceItemDto.FaceId);
                continue;
            }


            var newMemberFace = _mapper.Map<MemberFace>(importFaceItemDto);
            // newMemberFace.FamilyId = request.FamilyId; // FamilyId được gán qua Member.FamilyId
            _context.MemberFaces.Add(newMemberFace);
            importedMemberFaces.Add(newMemberFace);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Nạp lại các MemberFace đã nhập để có các trường được làm giàu
        var importedMemberFacesWithEnrichedData = await _context.MemberFaces
            .Where(mf => importedMemberFaces.Select(imf => imf.Id).Contains(mf.Id))
            .Include(mf => mf.Member)
                .ThenInclude(m => m!.Family)
            .ProjectTo<MemberFaceDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);


        return Result<List<MemberFaceDto>>.Success(importedMemberFacesWithEnrichedData);
    }
}
