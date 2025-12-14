using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembersByFamilyId;

/// <summary>
/// Xử lý truy vấn để lấy danh sách các thành viên thuộc một gia đình cụ thể.
/// </summary>
public class GetMembersByFamilyIdQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser currentUser, IMapper mapper) : IRequestHandler<GetMembersByFamilyIdQuery, Result<List<MemberDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Xử lý logic để lấy danh sách thành viên theo FamilyId.
    /// </summary>
    /// <param name="request">Đối tượng truy vấn GetMembersByFamilyIdQuery.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một Result chứa danh sách MemberListDto nếu thành công, ngược lại là lỗi.</returns>
    public async Task<Result<List<MemberDto>>> Handle(GetMembersByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy tất cả các liên kết gia đình liên quan đến request.FamilyId
        var linkedFamilyIds = await _context.FamilyLinks
            .Where(fl => fl.Family1Id == request.FamilyId || fl.Family2Id == request.FamilyId)
            .Select(fl => fl.Family1Id == request.FamilyId ? fl.Family2Id : fl.Family1Id)
            .ToListAsync(cancellationToken);

        // Bao gồm FamilyId gốc vào danh sách các ID cần truy vấn
        var allFamilyIdsToQuery = new List<Guid> { request.FamilyId };
        allFamilyIdsToQuery.AddRange(linkedFamilyIds);

        // 2. Lấy thành viên từ tất cả các gia đình liên quan và áp dụng MemberAccessSpecification
        var members = await _context.Members
            .AsNoTracking()
            .Where(m => allFamilyIdsToQuery.Contains(m.FamilyId))
            .WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId))
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MemberDto>>.Success(members);
    }
}
