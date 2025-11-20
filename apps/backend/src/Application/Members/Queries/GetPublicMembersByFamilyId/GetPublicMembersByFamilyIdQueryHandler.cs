using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Members.Queries.GetPublicMembersByFamilyId;

/// <summary>
/// Xử lý truy vấn để lấy danh sách các thành viên của một gia đình công khai theo Family ID.
/// </summary>
public class GetPublicMembersByFamilyIdQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<GetPublicMembersByFamilyIdQuery, Result<List<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<List<MemberListDto>>> Handle(GetPublicMembersByFamilyIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Verify if the family exists and is public
        var family = await _context.Families
            .AsNoTracking()
            .WithSpecification(new FamilyByIdSpecification(request.FamilyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<List<MemberListDto>>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        if (family.Visibility != FamilyVisibility.Public.ToString())
        {
            return Result<List<MemberListDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 2. Retrieve members for that family
        var members = await _context.Members
            .AsNoTracking()
            .WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId))
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // 3. Apply privacy filter to each member
        var filteredMembers = await _privacyService.ApplyPrivacyFilter(members, request.FamilyId, cancellationToken);

        return Result<List<MemberListDto>>.Success(filteredMembers);
    }
}
