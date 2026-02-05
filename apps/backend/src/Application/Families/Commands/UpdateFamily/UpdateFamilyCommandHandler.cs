using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Families.Specifications;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Domain.Entities; // NEW: Added for Location and LocationLink entities
using backend.Domain.Enums; // NEW: Added for RefType enum
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<UpdateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<Guid>> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanManageFamily(request.Id))
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);

        var entity = await _context.Families
            .WithSpecification(new FamilyByIdSpecification(request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Family with ID {request.Id}"), ErrorSources.NotFound);
        }

        string? finalAvatarUrl = entity.AvatarUrl; // Keep current avatar URL by default

        // --- Handle AvatarBase64 upload ---
        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var contentType = ImageUtils.GetMimeTypeFromBase64(request.AvatarBase64);

                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    RefId = entity.Id, // Link media to the updated Family
                    RefType = RefType.Family,
                    FamilyId = entity.Id,
                    MediaLinkType = MediaLinkType.Avatar,
                    AllowMultipleMediaLinks = false, // Avatars should not allow multiple links
                    File = imageData,
                    FileName = $"Family_Avatar_{Guid.NewGuid()}.png",
                    ContentType = contentType, // Use inferred content type
                    Folder = string.Format(UploadConstants.ImagesFolder, entity.Id),
                    MediaType = MediaType.Image // Explicitly set MediaType
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

        entity.UpdateFamilyDetails(
            request.Name,
            request.Description,
            request.Address,
            request.Visibility,
            entity.Code, // Pass the existing code, as it's not updated via this command input
            request.GenealogyRecord,
            request.ProgenitorName,
            request.FamilyCovenant,
            request.ContactInfo
        );
        entity.UpdateAvatar(finalAvatarUrl); // Update avatar using its specific method

        // --- Handle LocationLink ---
        var existingLocationLink = await _context.LocationLinks
            .FirstOrDefaultAsync(ll => ll.RefId == entity.Id.ToString() && ll.RefType == RefType.Family && ll.LinkType == LocationLinkType.General, cancellationToken); // NEW: Add LinkType to query

        if (request.LocationId.HasValue)
        {
            var location = await _context.Locations.FindAsync([request.LocationId.Value], cancellationToken);
            if (location == null)
            {
                return Result<Guid>.Failure($"Location with ID {request.LocationId.Value} not found.", ErrorSources.NotFound);
            }

            if (existingLocationLink == null)
            {
                // Create new link
                var newLocationLink = LocationLink.Create(
                    entity.Id.ToString(),
                    RefType.Family,
                    "Family Location", // Default description for Family Location Link
                    request.LocationId.Value,
                    LocationLinkType.General // NEW: Specify LinkType for family
                );
                _context.LocationLinks.Add(newLocationLink);
            }
            else
            {
                // Update existing link if the location ID has changed
                if (existingLocationLink.LocationId != request.LocationId.Value)
                {
                    existingLocationLink.Update(
                        existingLocationLink.RefId,
                        existingLocationLink.RefType,
                        existingLocationLink.Description,
                        request.LocationId.Value,
                        LocationLinkType.General // NEW: Specify LinkType for family
                    );
                }
            }
        }
        else // request.LocationId is null
        {
            if (existingLocationLink != null)
            {
                // Remove existing link if LocationId is now null
                _context.LocationLinks.Remove(existingLocationLink);
            }
        }
        // --- End Handle LocationLink ---

        // --- Update FamilyUsers ---
        // Clear all existing family users
        var familyUsersToRemove = _context.FamilyUsers.Where(fu => fu.FamilyId == entity.Id).ToList();
        _context.FamilyUsers.RemoveRange(familyUsersToRemove);
        entity.ClearFamilyUsers(); // Clear the collection in the aggregate root

        // Add Managers
        foreach (var managerId in request.ManagerIds.Distinct())
        {
            entity.AddFamilyUser(managerId, FamilyRole.Manager);
        }

        // Add Viewers
        foreach (var viewerId in request.ViewerIds.Distinct())
        {
            // Ensure a user isn't added as both manager and viewer.
            // If they are a manager, they implicitly have viewer permissions.
            if (!request.ManagerIds.Contains(viewerId))
            {
                entity.AddFamilyUser(viewerId, FamilyRole.Viewer);
            }
        }
        // --- End Update FamilyUsers ---

        entity.AddDomainEvent(new FamilyUpdatedEvent(entity));
        entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
