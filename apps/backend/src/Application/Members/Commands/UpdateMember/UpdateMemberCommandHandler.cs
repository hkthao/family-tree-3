using Ardalis.Specification.EntityFrameworkCore; // Re-add this
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Families.Specifications;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Domain.Entities; // NEW
using backend.Domain.Enums; // NEW
using backend.Domain.Events.Members;
using backend.Domain.ValueObjects; // NEW
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMemberRelationshipService memberRelationshipService, IMediator mediator, ILogger<UpdateMemberCommandHandler> logger) : IRequestHandler<UpdateMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMemberRelationshipService _memberRelationshipService = memberRelationshipService;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<UpdateMemberCommandHandler> _logger = logger;

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
                var contentType = ImageUtils.GetMimeTypeFromBase64(request.AvatarBase64);

                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = request.FamilyId, // Add FamilyId
                    File = imageData,
                    FileName = $"Member_Avatar_{Guid.NewGuid()}.png",
                    ContentType = contentType, // Use inferred content type
                    Folder = string.Format(UploadConstants.ImagesFolder, request.FamilyId),
                    MediaType = MediaType.Image // Explicitly set MediaType if known
                };

                var uploadResult = await _mediator.Send(createFamilyMediaCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result<Guid>.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                if (uploadResult.Value == null || string.IsNullOrEmpty(uploadResult.Value.FilePath))
                {
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                finalAvatarUrl = uploadResult.Value.FilePath; // Update finalAvatarUrl
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
            request.LunarDateOfDeath == null ? null : new LunarDate(request.LunarDateOfDeath.Day, request.LunarDateOfDeath.Month, request.LunarDateOfDeath.IsLeapMonth, request.LunarDateOfDeath.IsEstimated),
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

        // Synchronize Birth and Death events (now handled by MemberUpdatedEventHandler)

        // Conditionally add domain event based on request
        if (!request.SkipDomainEvent)
        {
            member.AddDomainEvent(new MemberUpdatedEvent(member));
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Cập nhật các mối quan hệ bằng phương thức mới
        await _memberRelationshipService.UpdateMemberRelationshipsAsync(
            member.Id,
            request.FatherId,
            request.MotherId,
            request.HusbandId,
            request.WifeId,
            cancellationToken
        );

        // --- Handle Location Links for Member ---
        await HandleLocationLink(member.Id, request.BirthLocationId, LocationLinkType.Birth, cancellationToken);
        await HandleLocationLink(member.Id, request.DeathLocationId, LocationLinkType.Death, cancellationToken);
        await HandleLocationLink(member.Id, request.ResidenceLocationId, LocationLinkType.Residence, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken); // Save all location links at once

        return Result<Guid>.Success(member.Id);
    }

    private async Task HandleLocationLink(Guid refId, Guid? locationId, LocationLinkType linkType, CancellationToken cancellationToken)
    {
        var existingLink = await _context.LocationLinks
            .FirstOrDefaultAsync(ll => ll.RefId == refId.ToString() && ll.RefType == RefType.Member && ll.LinkType == linkType, cancellationToken);

        if (locationId.HasValue)
        {
            var location = await _context.Locations.FindAsync(new object[] { locationId.Value }, cancellationToken);
            if (location == null)
            {
                // Log a warning if the provided locationId does not correspond to an existing location
                _logger.LogWarning("Location with ID {LocationId} not found for {LinkType} link for member {RefId}.", locationId.Value, linkType, refId);
                return;
            }

            if (existingLink == null)
            {
                // Create a new link if none exists
                var newLocationLink = LocationLink.Create(
                    refId.ToString(),
                    RefType.Member,
                    linkType.ToString(),
                    locationId.Value,
                    linkType
                );
                _context.LocationLinks.Add(newLocationLink);
            }
            else if (existingLink.LocationId != locationId.Value)
            {
                // Update existing link if the location has changed
                existingLink.Update(
                    existingLink.RefId,
                    existingLink.RefType,
                    existingLink.Description,
                    locationId.Value,
                    linkType
                );
            }
            // If existingLink is not null and LocationId is the same, no action is needed.
        }
        else
        {
            // If locationId is null, remove any existing link of this type for the member
            if (existingLink != null)
            {
                _context.LocationLinks.Remove(existingLink);
            }
        }
    }
}
