using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Specifications;
using backend.Domain.Enums;

namespace backend.Application.Members.Queries.GetPublicMemberById;

/// <summary>
/// Xử lý truy vấn để lấy thông tin chi tiết của một thành viên cụ thể trong một gia đình công khai.
/// </summary>
public class GetPublicMemberByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService) : IRequestHandler<GetPublicMemberByIdQuery, Result<MemberDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<MemberDetailDto>> Handle(GetPublicMemberByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Verify if the family exists and is public
        var family = await _context.Families
            .AsNoTracking()
            .WithSpecification(new FamilyByIdSpecification(request.FamilyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            return Result<MemberDetailDto>.Failure(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        if (family.Visibility != FamilyVisibility.Public.ToString())
        {
            return Result<MemberDetailDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 2. Retrieve the specific member, ensuring it belongs to the given familyId
        var spec = new MemberByIdSpecification(request.Id);
        spec.Query.Where(m => m.FamilyId == request.FamilyId); // Ensure member belongs to the specified family
        spec.Query.Include(m => m.SourceRelationships).ThenInclude(r => r.TargetMember);
        spec.Query.Include(m => m.TargetRelationships).ThenInclude(r => r.SourceMember);

        var memberDetailDto = await _context.Members
            .AsNoTracking()
            .WithSpecification(spec)
            // .AsSplitQuery() // Removed for debugging in-memory database issues
            .ProjectTo<MemberDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberDetailDto == null)
        {
            return Result<MemberDetailDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id} in Family ID {request.FamilyId}"), ErrorSources.NotFound);
        }

        // 3. Apply privacy filter
        var filteredMemberDetailDto = await _privacyService.ApplyPrivacyFilter(memberDetailDto, request.FamilyId, cancellationToken);

        return Result<MemberDetailDto>.Success(filteredMemberDetailDto);
    }
}
