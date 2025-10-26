using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events.Members;

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

        var entity = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Code = request.Code ?? GenerateUniqueCode("MEM"),
            Nickname = request.Nickname,
            DateOfBirth = request.DateOfBirth,
            DateOfDeath = request.DateOfDeath,
            PlaceOfBirth = request.PlaceOfBirth,
            PlaceOfDeath = request.PlaceOfDeath,
            Gender = request.Gender,
            AvatarUrl = request.AvatarUrl,
            Occupation = request.Occupation,
            Biography = request.Biography,
            FamilyId = request.FamilyId,
            IsRoot = request.IsRoot
        };

        if (request.IsRoot)
        {
            var currentRoot = await _context.Members
                .Where(m => m.FamilyId == request.FamilyId && m.IsRoot)
                .FirstOrDefaultAsync(cancellationToken);
            if (currentRoot != null)
                currentRoot.IsRoot = false;
        }

        _context.Members.Add(entity);
        entity.AddDomainEvent(new MemberCreatedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
