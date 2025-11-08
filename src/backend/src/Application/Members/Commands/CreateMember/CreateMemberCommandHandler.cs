using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities; // Import Relationship
using backend.Domain.Enums; // Import RelationshipType

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
            request.FamilyId,
            request.Nickname,
            request.Gender,
            request.DateOfBirth,
            request.DateOfDeath,
            request.PlaceOfBirth,
            request.PlaceOfDeath,
            request.Occupation,
            request.AvatarUrl,
            request.Biography
        );

        var member = family.AddMember(newMember, request.IsRoot);

        _context.Members.Add(member);

        // Handle FatherId
        if (request.FatherId.HasValue)
        {
            if (request.FatherId.Value == member.Id)
            {
                return Result<Guid>.Failure("A member cannot be their own father.", ErrorSources.BadRequest);
            }
            var father = await _context.Members.FindAsync(request.FatherId.Value);
            if (father != null)
            {
                var fatherChildRelationship = member.AddFatherRelationship(father.Id);
                _context.Relationships.Add(fatherChildRelationship);
            }
        }

        // Handle MotherId
        if (request.MotherId.HasValue)
        {
            if (request.MotherId.Value == member.Id)
            {
                return Result<Guid>.Failure("A member cannot be their own mother.", ErrorSources.BadRequest);
            }
            var mother = await _context.Members.FindAsync(request.MotherId.Value);
            if (mother != null)
            {
                var motherChildRelationship = member.AddMotherRelationship(mother.Id);
                _context.Relationships.Add(motherChildRelationship);
            }
        }

        // Handle HusbandId
        if (request.HusbandId.HasValue)
        {
            var husband = await _context.Members.FindAsync(request.HusbandId.Value);
            if (husband != null)
            {
                var currentToSpouse = member.AddHusbandRelationship(husband.Id);
                _context.Relationships.Add(currentToSpouse);
            }
        }

        // Handle WifeId
        if (request.WifeId.HasValue)
        {
            var wife = await _context.Members.FindAsync(request.WifeId.Value);
            if (wife != null)
            {
                var currentToSpouse = member.AddWifeRelationship(wife.Id);
                _context.Relationships.Add(currentToSpouse);
            }
        }

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
