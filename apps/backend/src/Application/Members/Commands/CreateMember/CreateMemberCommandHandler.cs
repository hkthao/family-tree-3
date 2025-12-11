using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia;
using backend.Domain.Enums;
using backend.Domain.Events.Families;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Localization;

namespace backend.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IStringLocalizer<CreateMemberCommandHandler> localizer, IMemberRelationshipService memberRelationshipService, IMediator mediator) : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IStringLocalizer<CreateMemberCommandHandler> _localizer = localizer;
    private readonly IMemberRelationshipService _memberRelationshipService = memberRelationshipService;
    private readonly IMediator _mediator = mediator;

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
            request.Phone,
            request.Email,
            request.Address,
            request.Occupation,
            request.AvatarUrl, // Keep AvatarUrl in constructor
            request.Biography,
            request.Order,
            request.IsDeceased
        );

        if (request.Id.HasValue)
        {
            newMember.SetId(request.Id.Value);
        }

        var member = family.AddMember(newMember, request.IsRoot);
        _context.Members.Add(member); // Add member to context before first save

        // --- Handle AvatarBase64 upload ---
        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = request.FamilyId, // Add FamilyId
                    File = imageData,
                    FileName = $"Member_Avatar_{Guid.NewGuid().ToString()}.png", // Use FileName property
                    Folder = string.Format(UploadConstants.MemberAvatarFolder, member.FamilyId),
                    MediaType = Domain.Enums.MediaType.Image // Explicitly set MediaType if known
                };

                var uploadResult = await _mediator.Send(createFamilyMediaCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result<Guid>.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                // CreateFamilyMediaCommand returns a Guid (the ID of the new FamilyMedia record), not an object with a Url.
                // We need to fetch the FamilyMedia object to get its FilePath (URL).
                var familyMedia = await _context.FamilyMedia.FindAsync(uploadResult.Value);
                if (familyMedia == null || string.IsNullOrEmpty(familyMedia.FilePath))
                {
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                member.UpdateAvatar(familyMedia.FilePath); // Update avatar after successful upload
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
        // --- End Handle AvatarBase64 upload ---

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

                // Automatically add the reverse Child relationship
                var childParentRelationship = new Domain.Entities.Relationship(member.FamilyId, member.Id, father.Id, RelationshipType.Child);
                _context.Relationships.Add(childParentRelationship);
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

                // Automatically add the reverse Child relationship
                var childParentRelationship = new Domain.Entities.Relationship(member.FamilyId, member.Id, mother.Id, RelationshipType.Child);
                _context.Relationships.Add(childParentRelationship);
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

        // Automatically create Birth and Death events
        if (member.DateOfBirth.HasValue)
        {
            var birthEvent = new Domain.Entities.Event(_localizer["Birth of {0}", member.FullName], $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}", EventType.Birth, member.FamilyId, member.DateOfBirth.Value);
            birthEvent.AddEventMember(member.Id);
            _context.Events.Add(birthEvent);
        }

        if (member.DateOfDeath.HasValue)
        {
            var deathEvent = new Domain.Entities.Event(_localizer["Death of {0}", member.FullName], $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}", EventType.Death, member.FamilyId, member.DateOfDeath.Value);
            deathEvent.AddEventMember(member.Id);
            _context.Events.Add(deathEvent);
        }

        member.AddDomainEvent(new MemberCreatedEvent(member));
        member.AddDomainEvent(new FamilyStatsUpdatedEvent(member.FamilyId));
        await _context.SaveChangesAsync(cancellationToken);

        // Update denormalized relationship fields after all relationships are established
        await _memberRelationshipService.UpdateDenormalizedRelationshipFields(member, cancellationToken);

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
