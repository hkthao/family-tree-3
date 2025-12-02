using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using backend.Domain.Entities;
using MediatR;
using backend.Application.Common.Utils; 

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

        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var uploadCommand = new UploadFileCommand
                {
                    ImageData = imageData,
                    FileName = $"Family_Avatar_{Guid.NewGuid()}.png",
                    Folder = string.Format(UploadConstants.FamilyAvatarFolder, entity.Id), // Use entity.Id now
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

                entity.UpdateAvatar(uploadResult.Value.Url); // Update avatar after successful upload
                await _context.SaveChangesAsync(cancellationToken); // Save avatar URL
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

        return Result<Guid>.Success(entity.Id);
    }

    private string GenerateUniqueCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
