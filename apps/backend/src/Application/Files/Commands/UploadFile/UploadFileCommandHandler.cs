using backend.Application.AI.DTOs; // NEW USING FOR IMAGELOADWEBHOOKDTO
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
namespace backend.Application.Files.UploadFile;

public class UploadFileCommandHandler(
    IN8nService n8nService
) : IRequestHandler<UploadFileCommand, Result<string>>
{
    private readonly IN8nService _n8nService = n8nService; // ADD NEW SERVICE FIELD

    public async Task<Result<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // For image uploads, we use the n8n webhook directly.
        // File type and size validations are now handled by n8n webhook or client-side.

        // 1. Construct ImageUploadWebhookDto
        var imageUploadDto = new ImageUploadWebhookDto
        {
            ImageData = request.ImageData,
            FileName = request.FileName,
            Cloud = request.Cloud,
            Folder = request.Folder
        };

        // 2. Call n8n Image Upload Webhook
        var n8nUploadResult = await _n8nService.CallImageUploadWebhookAsync(imageUploadDto, cancellationToken);
        if (!n8nUploadResult.IsSuccess)
        {
            return Result<string>.Failure(n8nUploadResult.Error ?? ErrorMessages.FileUploadFailed, n8nUploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        var uploadedImageUrl = n8nUploadResult.Value?.Url;

        if (string.IsNullOrEmpty(uploadedImageUrl))
        {
            return Result<string>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
        }

        // 3. (Optional) Save metadata to DB if needed, but not using FileMetadata table anymore for external uploads.
        // For now, just return the URL, assuming it will be stored as PhotoUrl in Memory entity later.

        return Result<string>.Success(uploadedImageUrl);
    }

    // Removed the SanitizeFileName method as it's no longer used.
}
