using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils; // NEW
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Domain.Entities;
using backend.Domain.Enums; // NEW
using backend.Domain.Events.Families;

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler(IApplicationDbContext context, ICurrentUser user, IMediator mediator) : IRequestHandler<CreateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _user = user;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<Guid>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        var entity = Family.Create(
            request.Name,
            request.Code ?? GenerateUniqueCode("FAM"),
            request.Description,
            request.Address,
            request.Visibility,
            _user.UserId,
            genealogyRecord: request.GenealogyRecord,
            progenitorName: request.ProgenitorName,
            familyCovenant: request.FamilyCovenant,
            contactInfo: request.ContactInfo
        );

        _context.Families.Add(entity);
        await _context.SaveChangesAsync(cancellationToken); // Save to get entity.Id

        foreach (var managerId in request.ManagerIds)
        {
            entity.AddFamilyUser(managerId, FamilyRole.Manager);
        }

        // Add Viewers
        foreach (var viewerId in request.ViewerIds)
        {
            entity.AddFamilyUser(viewerId, FamilyRole.Viewer);
        }

        entity.AddDomainEvent(new FamilyCreatedEvent(entity));
        entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var contentType = ImageUtils.GetMimeTypeFromBase64(request.AvatarBase64);

                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    RefId = entity.Id, // Link media to the newly created Family
                    RefType = RefType.Family,
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

        // Handle LocationLink
        if (request.LocationId.HasValue)
        {
            var locationLink = LocationLink.Create(
                entity.Id.ToString(), // RefId is FamilyId
                RefType.Family,       // RefType is Family
                string.Empty,         // Description
                request.LocationId.Value,
                LocationLinkType.General // NEW: Specify LinkType for family
            );
            _context.LocationLinks.Add(locationLink);
        }

        await _context.SaveChangesAsync(cancellationToken); // Save avatar URL and family users

        return Result<Guid>.Success(entity.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
