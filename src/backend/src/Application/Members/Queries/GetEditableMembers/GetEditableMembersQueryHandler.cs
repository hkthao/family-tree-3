using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetMembers; // Added
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Members.Queries.GetEditableMembers;

public class GetEditableMembersQueryHandler : IRequestHandler<GetEditableMembersQuery, Result<List<MemberListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetEditableMembersQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Result<List<MemberListDto>>> Handle(GetEditableMembersQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        if (userId == null)
        {
            return Result<List<MemberListDto>>.Failure("User not authenticated.");
        }

        // Get families where the current user is a manager
        var managedFamilyIds = await _context.FamilyUsers
            .Where(fu => fu.UserProfileId.ToString() == userId && (fu.Role == FamilyRole.Manager || fu.Role == FamilyRole.Admin))
            .Select(fu => fu.FamilyId)
            .ToListAsync(cancellationToken);

        // Get members belonging to the managed families
        var members = await _context.Members
            .Where(m => managedFamilyIds.Contains(m.FamilyId))
            .Include(m => m.Family)
            .Select(m => new MemberListDto
            {
                Id = m.Id,
                FullName = m.FullName,
                FamilyId = m.FamilyId,
                FamilyName = m.Family != null ? m.Family.Name : null,
                DateOfBirth = m.DateOfBirth,
                DateOfDeath = m.DateOfDeath,
                Gender = m.Gender,
                AvatarUrl = m.AvatarUrl
            })
            .ToListAsync(cancellationToken);

        return Result<List<MemberListDto>>.Success(members);
    }
}
