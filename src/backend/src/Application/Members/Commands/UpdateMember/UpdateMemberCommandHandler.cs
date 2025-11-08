using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Domain.Enums; // Import RelationshipType

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
            .WithSpecification(new FamilyByIdSpecification(request.FamilyId))
            .FirstOrDefaultAsync(cancellationToken);
        if (family == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {request.FamilyId}"), ErrorSources.NotFound);
        }

        var member = await _context.Members
            .Include(m => m.SourceRelationships)
            .Include(m => m.TargetRelationships)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Id}"), ErrorSources.NotFound);
        }

        member.Update(
            request.FirstName,
            request.LastName,
            member.Code,
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

        // Handle IsRoot property update
        if (request.IsRoot)
        {
            // If the updated member should be the root
            var currentRoot = _context.Members.FirstOrDefault(m => m.IsRoot && m.Id != member.Id);
            currentRoot?.UnsetAsRoot(); // Unset the old root if it exists
            member.SetAsRoot(); // Set the current member as the new root
        }
        else if (member.IsRoot) // If the member was previously a root but now shouldn't be
        {
            member.UnsetAsRoot();
        }

        // --- Simplify Relationship Handling ---
        // Remove all existing parent and spouse relationships for the member
        var relationshipsToRemove = _context.Relationships
            .Where(r => (r.TargetMemberId == member.Id && (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother)) ||
                        (r.SourceMemberId == member.Id && (r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)) ||
                        (r.TargetMemberId == member.Id && (r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)))
            .ToList();

        _context.Relationships.RemoveRange(relationshipsToRemove);

        // Re-add relationships from the request
        if (request.FatherId.HasValue)
        {
            if (request.FatherId.Value == member.Id)
            {
                return Result<Guid>.Failure("A member cannot be their own father.", ErrorSources.BadRequest);
            }
            var father = await _context.Members.FindAsync(request.FatherId.Value);
            if (father != null)
            {
                var newFatherRelationship = member.AddFatherRelationship(request.FatherId.Value);
                _context.Relationships.Add(newFatherRelationship);
            }
        }

        if (request.MotherId.HasValue)
        {
            if (request.MotherId.Value == member.Id)
            {
                return Result<Guid>.Failure("A member cannot be their own mother.", ErrorSources.BadRequest);
            }
            var mother = await _context.Members.FindAsync(request.MotherId.Value);
            if (mother != null)
            {
                var newMotherRelationship = member.AddMotherRelationship(request.MotherId.Value);
                _context.Relationships.Add(newMotherRelationship);
            }
        }

        if (request.HusbandId.HasValue)
        {
            var husband = await _context.Members.FindAsync(request.HusbandId.Value);
            if (husband != null)
            {
                if (member.Gender == null)
                {
                    return Result<Guid>.Failure("Member gender is required to establish spouse relationship.", ErrorSources.BadRequest);
                }
                var (currentToSpouse, spouseToCurrent) = member.AddSpouseRelationship(husband.Id, (Gender)Enum.Parse(typeof(Gender), member.Gender));
                _context.Relationships.Add(currentToSpouse);
                _context.Relationships.Add(spouseToCurrent);
            }
        }

        if (request.WifeId.HasValue)
        {
            var wife = await _context.Members.FindAsync(request.WifeId.Value);
            if (wife != null)
            {
                if (member.Gender == null)
                {
                    return Result<Guid>.Failure("Member gender is required to establish spouse relationship.", ErrorSources.BadRequest);
                }
                var (currentToSpouse, spouseToCurrent) = member.AddSpouseRelationship(wife.Id, (Gender)Enum.Parse(typeof(Gender), member.Gender));
                _context.Relationships.Add(currentToSpouse);
                _context.Relationships.Add(spouseToCurrent);
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message, "DbUpdate");
        }

        return Result<Guid>.Success(member.Id);
    }
}
