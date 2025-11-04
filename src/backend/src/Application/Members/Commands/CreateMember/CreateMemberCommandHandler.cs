using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Events.Members;
using backend.Domain.Events.Families;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        // If the user has the 'Admin' role, bypass family-specific access checks
        if (!_authorizationService.CanManageFamily(request.FamilyId))
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        var family = await _context.Families.FindAsync(request.FamilyId);
        if (family == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {request.FamilyId}"), ErrorSources.NotFound);
        }

        var newMember = new Domain.Entities.Member(
            request.LastName,
            request.FirstName,
            request.Code ?? GenerateUniqueCode("MEM"),
            request.FamilyId
        );

        newMember.Nickname = request.Nickname;
        newMember.DateOfBirth = request.DateOfBirth;
        newMember.DateOfDeath = request.DateOfDeath;
        newMember.PlaceOfBirth = request.PlaceOfBirth;
        newMember.PlaceOfDeath = request.PlaceOfDeath;
        newMember.Gender = request.Gender;
        newMember.AvatarUrl = request.AvatarUrl;
        newMember.Occupation = request.Occupation;
        newMember.Biography = request.Biography;

        var member = family.AddMember(newMember, request.IsRoot);

        _context.Members.Add(member);

        member.AddDomainEvent(new MemberCreatedEvent(member));
        family.AddDomainEvent(new FamilyStatsUpdatedEvent(family.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(member.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
