using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Files.UploadFile; // Import UploadFileCommand
using backend.Domain.Entities;

using backend.Application.Common.Utils; // NEW

namespace backend.Application.Families.Commands.CreateFamily;

public class CreateFamilyCommandHandler(IApplicationDbContext context, ICurrentUser user, IMediator mediator) : IRequestHandler<CreateFamilyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUser _user = user;
    private readonly IMediator _mediator = mediator; // Inject IMediator

    public async Task<Result<Guid>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        string? avatarUrl = null;
        if (!string.IsNullOrEmpty(request.AvatarBase64))
        {
            try
            {
                var imageData = ImageUtils.ConvertBase64ToBytes(request.AvatarBase64);
                var uploadCommand = new UploadFileCommand
                {
                    ImageData = imageData,
                    FileName = $"Family_Avatar_{Guid.NewGuid().ToString()}.png",
                    Folder = "family-avatars",
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

                avatarUrl = uploadResult.Value.Url;
            }
            catch (FormatException)
            {
                return Result<Guid>.Failure(ErrorMessages.InvalidBase64, ErrorSources.Validation);
            }
            catch (Exception ex)
            {
                // Log the exception details here if a logger is available
                return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
            }
        }

        try
        {
            var entity = Family.Create(
                request.Name,
                request.Code ?? GenerateUniqueCode("FAM"),
                request.Description,
                request.Address,
                request.Visibility,
                _user.UserId
            );

            if (avatarUrl != null)
            {
                entity.UpdateAvatar(avatarUrl);
            }

            _context.Families.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entity.Id);
        }
        catch (Exception ex)
        {
            // Log the exception details here if a logger is available
            return Result<Guid>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }

    private string GenerateUniqueCode(string prefix)
    {
        // For simplicity, generate a GUID and take a substring.
        // In a real application, you'd want to ensure uniqueness against existing codes in the database.
        return $"{prefix}-{Guid.NewGuid().ToString()[..5].ToUpper()}";
    }
}
