using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Specifications;
using backend.Application.Members.Specifications;
using backend.Domain.Entities; // Import Relationship
using backend.Domain.Enums; // Import RelationshipType
using Microsoft.EntityFrameworkCore; // Import for Include

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
            .Include(m => m.Relationships) // Include relationships to manage them
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

        // --- Handle Father Relationship ---
        var existingFatherRelationship = member.Relationships
            .FirstOrDefault(r => r.TargetMemberId == member.Id && r.Type == RelationshipType.Father);

        if (request.FatherId.HasValue)
        {
            if (existingFatherRelationship != null)
            {
                // Update existing relationship if father changed
                if (existingFatherRelationship.SourceMemberId != request.FatherId.Value)
                {
                    _context.Relationships.Remove(existingFatherRelationship);
                    var newFatherRelationship = member.AddFatherRelationship(request.FatherId.Value);
                    _context.Relationships.Add(newFatherRelationship);
                }
            }
            else
            {
                // Add new father relationship
                var newFatherRelationship = member.AddFatherRelationship(request.FatherId.Value);
                _context.Relationships.Add(newFatherRelationship);
            }
        }
        else if (existingFatherRelationship != null)
        {
            // Remove existing father relationship if FatherId is no longer provided
            _context.Relationships.Remove(existingFatherRelationship);
        }

        // --- Handle Mother Relationship ---
        var existingMotherRelationship = member.Relationships
            .FirstOrDefault(r => r.TargetMemberId == member.Id && r.Type == RelationshipType.Mother);

        if (request.MotherId.HasValue)
        {
            if (existingMotherRelationship != null)
            {
                // Update existing relationship if mother changed
                if (existingMotherRelationship.SourceMemberId != request.MotherId.Value)
                {
                    _context.Relationships.Remove(existingMotherRelationship);
                    var newMotherRelationship = member.AddMotherRelationship(request.MotherId.Value);
                    _context.Relationships.Add(newMotherRelationship);
                }
            }
            else
            {
                // Add new mother relationship
                var newMotherRelationship = member.AddMotherRelationship(request.MotherId.Value);
                _context.Relationships.Add(newMotherRelationship);
            }
        }
        else if (existingMotherRelationship != null)
        {
            // Remove existing mother relationship if MotherId is no longer provided
            _context.Relationships.Remove(existingMotherRelationship);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(member.Id);
    }
}
