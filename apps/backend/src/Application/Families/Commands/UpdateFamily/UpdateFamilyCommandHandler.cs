using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Families.Specifications;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia; // NEW
using backend.Domain.Events.Families;
using backend.Domain.ValueObjects; // NEW
using Microsoft.Extensions.Localization;

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
                var createFamilyMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = entity.Id, // Link media to the updated Family
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
                var familyMedia = await _context.FamilyMedia.FindAsync(uploadResult.Value);
                if (familyMedia == null || string.IsNullOrEmpty(familyMedia.FilePath))
                {
                    return Result<Guid>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                }

                finalAvatarUrl = familyMedia.FilePath; // Update finalAvatarUrl
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
            entity.Code // Pass the existing code, as it's not updated via this command input
        );
        entity.UpdateAvatar(finalAvatarUrl); // Update avatar using its specific method

        // --- Update FamilyUsers ---
        var familyUserUpdateInfos = request.FamilyUsers
            .Select(fu => new FamilyUserUpdateInfo(fu.UserId, fu.Role))
            .ToList();
        entity.UpdateFamilyUsers(familyUserUpdateInfos);
        // --- End Update FamilyUsers ---

        entity.AddDomainEvent(new FamilyUpdatedEvent(entity));
        entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
