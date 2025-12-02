using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Families.Specifications;
using backend.Application.Files.UploadFile; // NEW
using backend.Domain.Enums;
using backend.Domain.Events.Families; // NEW
using backend.Domain.Events.Members;
using Microsoft.Extensions.Localization;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<UpdateMemberCommandHandler> localizer, IMemberRelationshipService memberRelationshipService, IMediator mediator) : IRequestHandler<UpdateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IStringLocalizer<UpdateMemberCommandHandler> _localizer = localizer;
    private readonly IMemberRelationshipService _memberRelationshipService = memberRelationshipService;
    private readonly IMediator _mediator = mediator; // NEW

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

        string? finalAvatarUrl = member.AvatarUrl; // Keep current avatar URL by default

        // --- Handle AvatarBase64 upload ---
        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var uploadCommand = new UploadFileCommand
                {
                    ImageData = imageData,
                    FileName = $"Member_Avatar_{Guid.NewGuid()}.png",
                    Folder = string.Format(UploadConstants.MemberAvatarFolder, member.FamilyId),
                    ContentType = "image/png"
                };

                var uploadResult = await _mediator.Send(uploadCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result<Guid>.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                if (uploadResult.Value == null || string.IsNullOrEmpty(uploadResult.Value.Url))
                {
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                finalAvatarUrl = uploadResult.Value.Url; // Update finalAvatarUrl
            }
            catch (FormatException)
            {
                return Result<Guid>.Failure(ErrorMessages.InvalidBase64, ErrorSources.Validation);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
            }
        }
        else if (request.AvatarBase64 != null && request.AvatarBase64.Length == 0)
        {
            finalAvatarUrl = null; // Clear avatar if empty base64 is provided
        }
        // --- End Handle AvatarBase64 upload ---

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
            request.Phone,
            request.Email,
            request.Address,
            request.Occupation,
            finalAvatarUrl, // Pass finalAvatarUrl to update method
            request.Biography,
            request.Order,
            request.IsDeceased
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

        // Get existing relationship IDs
        var existingFatherId = member.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Father)?.SourceMemberId;
        var existingMotherId = member.TargetRelationships.FirstOrDefault(r => r.Type == RelationshipType.Mother)?.SourceMemberId;
        var existingHusbandId = member.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Wife)?.TargetMemberId; // Corrected: current member is wife of husband
        var existingWifeId = member.SourceRelationships.FirstOrDefault(r => r.Type == RelationshipType.Husband)?.TargetMemberId; // Corrected: current member is husband of wife

        // Handle Father relationship
        if (request.FatherId != existingFatherId)
        {
            // Remove old father relationship
            if (existingFatherId.HasValue)
            {
                var oldFatherRelationship = _context.Relationships
                    .FirstOrDefault(r => r.SourceMemberId == existingFatherId.Value && r.TargetMemberId == member.Id && r.Type == RelationshipType.Father);
                if (oldFatherRelationship != null)
                {
                    member.TargetRelationships.Remove(oldFatherRelationship); // Explicitly remove from navigation property
                    _context.Relationships.Remove(oldFatherRelationship);
                }
            }
            // Add new father relationship
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
        }

        // Handle Mother relationship
        if (request.MotherId != existingMotherId)
        {
            // Remove old mother relationship
            if (existingMotherId.HasValue)
            {
                var oldMotherRelationship = _context.Relationships
                    .FirstOrDefault(r => r.SourceMemberId == existingMotherId.Value && r.TargetMemberId == member.Id && r.Type == RelationshipType.Mother);
                if (oldMotherRelationship != null)
                {
                    member.TargetRelationships.Remove(oldMotherRelationship); // Explicitly remove from navigation property
                    _context.Relationships.Remove(oldMotherRelationship);
                }
            }
            // Add new mother relationship
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
        }

        // Handle Husband relationship
        if (request.HusbandId != existingHusbandId)
        {
            // Remove old husband relationship
            if (existingHusbandId.HasValue)
            {
                var oldHusbandRelationship = _context.Relationships
                    .FirstOrDefault(r => r.SourceMemberId == member.Id && r.TargetMemberId == existingHusbandId.Value && r.Type == RelationshipType.Wife);
                if (oldHusbandRelationship != null)
                {
                    member.SourceRelationships.Remove(oldHusbandRelationship); // Explicitly remove from navigation property
                    _context.Relationships.Remove(oldHusbandRelationship);
                }
            }
            // Add new husband relationship
            if (request.HusbandId.HasValue)
            {
                var husband = await _context.Members.FindAsync(request.HusbandId.Value);
                if (husband != null)
                {
                    var newHusbandRelationship = member.AddHusbandRelationship(husband.Id);
                    _context.Relationships.Add(newHusbandRelationship);
                }
            }
        }

        // Handle Wife relationship
        if (request.WifeId != existingWifeId)
        {
            // Remove old wife relationship
            if (existingWifeId.HasValue)
            {
                var oldWifeRelationship = _context.Relationships
                    .FirstOrDefault(r => r.SourceMemberId == member.Id && r.TargetMemberId == existingWifeId.Value && r.Type == RelationshipType.Husband);
                if (oldWifeRelationship != null)
                {
                    member.SourceRelationships.Remove(oldWifeRelationship); // Explicitly remove from navigation property
                    _context.Relationships.Remove(oldWifeRelationship);
                }
            }
            // Add new wife relationship
            if (request.WifeId.HasValue)
            {
                var wife = await _context.Members.FindAsync(request.WifeId.Value);
                if (wife != null)
                {
                    var newWifeRelationship = member.AddWifeRelationship(wife.Id);
                    _context.Relationships.Add(newWifeRelationship);
                }
            }
        }

        // Synchronize Birth and Death events
        await SyncLifeEvents(request, member, cancellationToken);

        // Update denormalized relationship fields after all relationships are established
        await _memberRelationshipService.UpdateDenormalizedRelationshipFields(member, cancellationToken);

        member.AddDomainEvent(new MemberUpdatedEvent(member));
        member.AddDomainEvent(new FamilyStatsUpdatedEvent(member.FamilyId));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(member.Id);
    }

    private async Task SyncLifeEvents(UpdateMemberCommand request, Domain.Entities.Member member, CancellationToken cancellationToken)
    {
        // Find existing birth and death events for the member
        var birthEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Birth && e.EventMembers.Any(em => em.MemberId == member.Id), cancellationToken);

        var deathEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Death && e.EventMembers.Any(em => em.MemberId == member.Id), cancellationToken);

        // Handle Birth Event
        if (request.DateOfBirth.HasValue)
        {
            if (birthEvent != null)
            {
                // Update existing birth event
                birthEvent.UpdateEvent(birthEvent.Name, birthEvent.Code, birthEvent.Description, request.DateOfBirth.Value, birthEvent.EndDate, birthEvent.Location, birthEvent.Type, birthEvent.Color);
            }
            else
            {
                // Create new birth event
                var newBirthEvent = new Domain.Entities.Event(_localizer["Birth of {0}", member.FullName], $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}", EventType.Birth, member.FamilyId, request.DateOfBirth.Value);
                newBirthEvent.AddEventMember(member.Id);
                _context.Events.Add(newBirthEvent);
            }
        }
        else if (birthEvent != null)
        {
            // Remove existing birth event if date is cleared
            _context.Events.Remove(birthEvent);
        }

        // Handle Death Event
        if (request.DateOfDeath.HasValue)
        {
            if (deathEvent != null)
            {
                // Update existing death event
                deathEvent.UpdateEvent(deathEvent.Name, deathEvent.Code, deathEvent.Description, request.DateOfDeath.Value, deathEvent.EndDate, deathEvent.Location, deathEvent.Type, deathEvent.Color);
            }
            else
            {
                // Create new death event
                var newDeathEvent = new Domain.Entities.Event(_localizer["Death of {0}", member.FullName], $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}", EventType.Death, member.FamilyId, request.DateOfDeath.Value);
                newDeathEvent.AddEventMember(member.Id);
                _context.Events.Add(newDeathEvent);
            }
        }
        else if (deathEvent != null)
        {
            // Remove existing death event if date is cleared
            _context.Events.Remove(deathEvent);
        }
    }
}
