using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUser user, IAuthorizationService authorizationService) : IRequestHandler<GetMembersQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUser  _user = user;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members.AsQueryable();

        // If the user has the 'Admin' role, bypass family-specific access checks
        if (_authorizationService.IsAdmin())
        {
            // Admin has access to all members, no further filtering by user profile needed
            // If a specific FamilyId is requested, still apply that filter
            if (request.FamilyId != Guid.Empty)
            {
                query = query.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId));
            }
        }
        else
        {
            // Get IDs of families the user has access to
            var accessibleFamilyIds = _context.FamilyUsers.Where(e=>e.UserId == _user.UserId).Select(fu => fu.FamilyId).ToList();

            // Apply family access filter if a specific FamilyId is requested
            if (request.FamilyId.HasValue && request.FamilyId.Value != Guid.Empty)
            {
                // Check if the requested FamilyId is among the user's accessible families
                if (!accessibleFamilyIds.Contains(request.FamilyId.Value))
                {
                    return Result<IReadOnlyList<MemberListDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
                }
                query = query.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId.Value));
            }
            else
            {
                // If no specific FamilyId is requested, filter by all families the user has access to
                query = query.Where(m => accessibleFamilyIds.Contains(m.FamilyId));
            }
        }

        // Apply other specifications
        query = query.WithSpecification(new MemberSearchTermSpecification(request.SearchTerm));

        var memberList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}
