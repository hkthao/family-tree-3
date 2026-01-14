using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMemberByFamilyIdAndCode;

public class GetMemberByFamilyIdAndCodeQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser, IPrivacyService privacyService) : IRequestHandler<GetMemberByFamilyIdAndCodeQuery, Result<MemberDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<MemberDetailDto>> Handle(GetMemberByFamilyIdAndCodeQuery request, CancellationToken cancellationToken)
    {
        // Áp dụng MemberAccessSpecification trước
        var baseQuery = _context.Members.AsQueryable()
            .WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

        var spec = new MemberByFamilyIdAndCodeSpecification(request.FamilyId, request.MemberCode);
        spec.Query.Include(m => m.SourceRelationships);
        spec.Query.Include(m => m.TargetRelationships);
        spec.Query.Include(m => m.Family);

        // Áp dụng MemberByFamilyIdAndCodeSpecification vào baseQuery đã được lọc
        var query = baseQuery.WithSpecification(spec);

        var memberDetailDto = await query
            .ProjectTo<MemberDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberDetailDto == null)
        {
            return Result<MemberDetailDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with Family ID {request.FamilyId} and Code {request.MemberCode}"), ErrorSources.NotFound);
        }

        // Áp dụng bộ lọc bảo mật
        var filteredMemberDetailDto = await _privacyService.ApplyPrivacyFilter(memberDetailDto, memberDetailDto.FamilyId, cancellationToken);

        return Result<MemberDetailDto>.Success(filteredMemberDetailDto);
    }
}
