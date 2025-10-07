using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Specifications;
using backend.Application.Members.Specifications;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, Result<IReadOnlyList<MemberListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetMembersQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<Result<IReadOnlyList<MemberListDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return Result<IReadOnlyList<MemberListDto>>.Failure("User is not authenticated.");
        }

        var currentUserProfile = await _context.UserProfiles
            .WithSpecification(new UserProfileByAuth0IdSpec(_user.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUserProfile == null)
        {
            return Result<IReadOnlyList<MemberListDto>>.Success(new List<MemberListDto>());
        }

        // Get IDs of families the user has access to
        var accessibleFamilyIds = currentUserProfile.FamilyUsers.Select(fu => fu.FamilyId).ToList();

        var query = _context.Members.AsQueryable();

        // Apply family access filter if a specific FamilyId is requested
        if (request.FamilyId != Guid.Empty)
        {
            // Check if the requested FamilyId is among the user's accessible families
            if (!accessibleFamilyIds.Contains(request.FamilyId.GetValueOrDefault()))
            {
                return Result<IReadOnlyList<MemberListDto>>.Failure("Access denied to the requested family.");
            }
            query = query.WithSpecification(new MemberByFamilyIdSpecification(request.FamilyId));
        }
        else
        {
            // If no specific FamilyId is requested, filter by all families the user has access to
            query = query.Where(m => accessibleFamilyIds.Contains(m.FamilyId));
        }

        // Apply other specifications
        query = query.WithSpecification(new MemberSearchTermSpecification(request.SearchTerm));
        // Note: GetMembersQuery does not have explicit sorting or pagination parameters beyond what PaginatedQuery provides.
        // If sorting is needed, a separate MemberOrderingSpecification would be applied here.
        // Pagination is handled by the PaginatedListAsync extension method.

        var memberList = await query
            .ProjectTo<MemberListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MemberListDto>>.Success(memberList);
    }
}
