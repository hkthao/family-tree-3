using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Domain.Entities;
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
            _user.UserId
        );

        _context.Families.Add(entity);
        await _context.SaveChangesAsync(cancellationToken); // Save to get entity.Id

        // Add Managers
        foreach (var managerId in request.ManagerIds)
        {
            if (managerId != _user.UserId) // The creator is already added as a manager in Family.Create
            {
                entity.AddFamilyUser(managerId, Domain.Enums.FamilyRole.Manager);
            }
        }

        // Add Viewers
        foreach (var viewerId in request.ViewerIds)
        {
            entity.AddFamilyUser(viewerId, Domain.Enums.FamilyRole.Viewer);
        }

        entity.AddDomainEvent(new FamilyCreatedEvent(entity));
        entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = entity.Id, // Link media to the newly created Family
                    File = imageData,
                    FileName = $"Family_Avatar_{Guid.NewGuid()}.png",
                    Folder = string.Format(UploadConstants.FamilyAvatarFolder, entity.Id),
                    MediaType = Domain.Enums.MediaType.Image // Explicitly set MediaType
                };

                var uploadResult = await _mediator.Send(createFamilyMediaCommand, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return Result<Guid>.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                }

                // CreateFamilyMediaCommand returns a Guid (the ID of the new FamilyMedia record).
                // We need to fetch the FamilyMedia object to get its FilePath (URL).
                var familyMedia = await _context.FamilyMedia.FindAsync(uploadResult.Value!.Id);
                if (familyMedia == null || string.IsNullOrEmpty(familyMedia.FilePath))
                {
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                entity.UpdateAvatar(familyMedia.FilePath); // Update avatar after successful upload
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

        await _context.SaveChangesAsync(cancellationToken); // Save avatar URL and family users

        return Result<Guid>.Success(entity.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
