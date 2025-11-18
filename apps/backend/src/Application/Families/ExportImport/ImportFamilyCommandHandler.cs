using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Families.ExportImport;

public class ImportFamilyCommandHandler : IRequestHandler<ImportFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;

    public ImportFamilyCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, IDateTime dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<Result<Guid>> Handle(ImportFamilyCommand request, CancellationToken cancellationToken)
    {
        if (request.FamilyData == null)
        {
            return Result<Guid>.Failure("Family data cannot be null.", ErrorSources.Validation);
        }

        if (request.FamilyId == null)
        {
            return Result<Guid>.Failure("FamilyId must be provided to update an existing family.", ErrorSources.Validation);
        }

        var familyToUpdate = await _context.Families
            .Include(f => f.Members)
            .Include(f => f.Relationships)
            .Include(f => f.Events)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId.Value, cancellationToken);

        if (familyToUpdate == null)
        {
            return Result<Guid>.Failure($"Family with ID '{request.FamilyId.Value}' not found.", ErrorSources.NotFound);
        }

        // Clear existing data
        if (request.ClearExistingData)
        {
            _context.Events.RemoveRange(familyToUpdate.Events);
            _context.Relationships.RemoveRange(familyToUpdate.Relationships);
            _context.Members.RemoveRange(familyToUpdate.Members);
        }

        // Update family properties
        familyToUpdate.UpdateFamilyDetails(
            request.FamilyData.Name,
            request.FamilyData.Description,
            request.FamilyData.Address,
            request.FamilyData.AvatarUrl,
            request.FamilyData.Visibility,
            familyToUpdate.Code // Keep existing code
        );


        // Map old member IDs to new member IDs
        var memberIdMap = new Dictionary<Guid, Guid>();

        // 2. Create new Member entities
        foreach (var memberDto in request.FamilyData.Members)
        {
            var newMember = new Member(
                memberDto.LastName,
                memberDto.FirstName,
                memberDto.Code ?? GenerateUniqueCode("MEM"),
                familyToUpdate.Id, // Assign to the new family
                memberDto.Nickname,
                memberDto.Gender?.ToString(),
                memberDto.DateOfBirth,
                memberDto.DateOfDeath,
                memberDto.PlaceOfBirth,
                memberDto.PlaceOfDeath,
                memberDto.Phone,
                memberDto.Email,
                memberDto.Address,
                memberDto.Occupation,
                memberDto.AvatarUrl,
                memberDto.Biography,
                memberDto.Order
            );

            // Generate new GUID for the imported member
            var oldMemberId = memberDto.Id;
            newMember.Id = Guid.NewGuid();
            memberIdMap[oldMemberId] = newMember.Id; // Store mapping

            newMember.Created = _dateTime.Now;
            newMember.CreatedBy = _currentUser.UserId.ToString();
            newMember.LastModified = _dateTime.Now;
            newMember.LastModifiedBy = _currentUser.UserId.ToString();

            _context.Members.Add(newMember);
        }

        // 3. Create new Relationship entities, using new member IDs
        foreach (var relationshipDto in request.FamilyData.Relationships)
        {
            if (!memberIdMap.TryGetValue(relationshipDto.SourceMemberId, out var newSourceMemberId) ||
                !memberIdMap.TryGetValue(relationshipDto.TargetMemberId, out var newTargetMemberId))
            {
                // This should ideally not happen if data is consistent, but handle gracefully
                return Result<Guid>.Failure($"Failed to map member IDs for relationship {relationshipDto.Id}.", ErrorSources.Validation);
            }

            var newRelationship = new Relationship(
                familyToUpdate.Id, // familyId
                newSourceMemberId, // sourceMemberId
                newTargetMemberId, // targetMemberId
                relationshipDto.Type, // type
                relationshipDto.Order // order
            );

            newRelationship.Id = Guid.NewGuid();
            newRelationship.Created = _dateTime.Now;
            newRelationship.CreatedBy = _currentUser.UserId.ToString();
            newRelationship.LastModified = _dateTime.Now;
            newRelationship.LastModifiedBy = _currentUser.UserId.ToString();

            _context.Relationships.Add(newRelationship);
        }

        // 4. Create new Event entities, using new member IDs for related members
        foreach (var eventDto in request.FamilyData.Events)
        {
            var newEvent = new Event(
                eventDto.Name,
                eventDto.Code ?? GenerateUniqueCode("EVT"),
                eventDto.Type,
                familyToUpdate.Id // Assign to the new family
            );

            newEvent.UpdateEvent(
                eventDto.Name,
                newEvent.Code, // Use the code generated or provided
                eventDto.Description,
                eventDto.StartDate,
                eventDto.EndDate,
                eventDto.Location,
                eventDto.Type,
                eventDto.Color
            );

            newEvent.Id = Guid.NewGuid();
            newEvent.Created = _dateTime.Now;
            newEvent.CreatedBy = _currentUser.UserId.ToString();
            newEvent.LastModified = _dateTime.Now;
            newEvent.LastModifiedBy = _currentUser.UserId.ToString();

            // Add related members to the event
            foreach (var oldRelatedMemberId in eventDto.RelatedMembers)
            {
                if (memberIdMap.TryGetValue(oldRelatedMemberId, out var newRelatedMemberId))
                {
                    newEvent.AddEventMember(newRelatedMemberId);
                }
            }

            _context.Events.Add(newEvent);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate family stats after import
        familyToUpdate.RecalculateStats();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(familyToUpdate.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
