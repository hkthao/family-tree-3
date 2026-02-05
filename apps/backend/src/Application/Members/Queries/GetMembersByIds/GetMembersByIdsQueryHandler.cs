using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser, IPrivacyService privacyService) : IRequestHandler<GetMembersByIdsQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IPrivacyService _privacyService = privacyService;

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersByIdsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members
            .WithSpecification(new MemberAccessSpecification(_authorizationService.IsAdmin(), _currentUser.UserId));

        var memberEntities = await query
            .WithSpecification(new MembersByIdsSpecification(request.Ids)) // Apply IDs specification after access
            .ToListAsync(cancellationToken);

        var memberList = _mapper.Map<List<MemberListDto>>(memberEntities);

        var filteredMemberList = new List<MemberListDto>();
        foreach (var memberListDto in memberList)
        {
            filteredMemberList.Add(await _privacyService.ApplyPrivacyFilter(memberListDto, memberListDto.FamilyId, cancellationToken));
        }

        return Result<IReadOnlyList<MemberListDto>>.Success(filteredMemberList);
    }
}
