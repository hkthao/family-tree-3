using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Families.Specifications;
using backend.Application.Files.UploadFile;
using backend.Domain.Events.Families; // NEW
using backend.Domain.ValueObjects;

namespace backend.Application.Families.Commands.UpdateFamily;

public class UpdateFamilyCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMediator mediator) : IRequestHandler<UpdateFamilyCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly IMediator _mediator = mediator; // Inject IMediator

    public async Task<Result> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_authorizationService.CanManageFamily(request.Id))
            {
                return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }

            var entity = await _context.Families.WithSpecification(new FamilyByIdSpecification(request.Id)).FirstOrDefaultAsync(cancellationToken);
            if (entity == null)
                return Result.Failure(string.Format(ErrorMessages.FamilyNotFound, request.Id), ErrorSources.NotFound);


            if (!string.IsNullOrEmpty(request.AvatarBase64))
            {
                try
                {
                    var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                    var uploadCommand = new UploadFileCommand
                    {
                        ImageData = imageData,
                        FileName = $"Family_Avatar_{Guid.NewGuid().ToString()}.png",
                        Folder = string.Format(UploadConstants.FamilyAvatarFolder, entity.Id), // Use UploadConstants
                        ContentType = "image/png"
                    };

                    var uploadResult = await _mediator.Send(uploadCommand, cancellationToken);

                    if (!uploadResult.IsSuccess)
                    {
                        return Result.Failure(string.Format(ErrorMessages.FileUploadFailed, uploadResult.Error), ErrorSources.FileUpload);
                    }

                    if (uploadResult.Value == null || string.IsNullOrEmpty(uploadResult.Value.Url))
                    {
                        return Result.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.FileUpload);
                    }
                    entity.UpdateAvatar(uploadResult.Value.Url);
                }
                catch (FormatException)
                {
                    return Result.Failure(ErrorMessages.InvalidBase64, ErrorSources.Validation);
                }
                catch (Exception ex)
                {
                    return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
                }
            }

            entity.UpdateFamilyDetails(request.Name, request.Description, request.Address, request.Visibility, request.Code!);
            _context.FamilyUsers.RemoveRange(entity.FamilyUsers);
            var familyUserUpdateInfos = request.FamilyUsers.Select(fu => new FamilyUserUpdateInfo(fu.UserId, fu.Role));
            entity.UpdateFamilyUsers(familyUserUpdateInfos);

            entity.AddDomainEvent(new FamilyUpdatedEvent(entity));
            entity.AddDomainEvent(new FamilyStatsUpdatedEvent(entity.Id));

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }
}
