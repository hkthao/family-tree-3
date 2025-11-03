using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Members;
using backend.Domain.Events.Families;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<UpdateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    public async Task<Result<Guid>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.FamilyId))
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        var family = await _context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {request.FamilyId}"), ErrorSources.NotFound);
        }

        var member = family.Members.FirstOrDefault(m => m.Id == request.Id);
        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id}"), ErrorSources.NotFound);
        }

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Nickname = request.Nickname;
        member.DateOfBirth = request.DateOfBirth;
        member.DateOfDeath = request.DateOfDeath;
        member.PlaceOfBirth = request.PlaceOfBirth;
        member.PlaceOfDeath = request.PlaceOfDeath;
        member.Gender = request.Gender;
        member.AvatarUrl = request.AvatarUrl;
        member.Occupation = request.Occupation;
        member.Biography = request.Biography;
        member.IsRoot = request.IsRoot;

        if (request.IsRoot)
        {
            var currentRoot = family.Members.FirstOrDefault(m => m.IsRoot && m.Id != request.Id);
            if (currentRoot != null)
            {
                currentRoot.IsRoot = false;
            }
        }

        member.AddDomainEvent(new MemberUpdatedEvent(member));
        family.AddDomainEvent(new FamilyStatsUpdatedEvent(request.FamilyId));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(member.Id);
    }
}
