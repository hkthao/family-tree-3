using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
        var members = await _context.Members
            .AsNoTracking()
            .Where(m => m.FamilyId == request.FamilyId)
            .WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId))
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MemberDto>>.Success(members);
    }
}