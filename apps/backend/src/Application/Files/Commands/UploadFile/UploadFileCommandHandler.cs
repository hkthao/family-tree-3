using backend.Application.AI.DTOs; // NEW USING FOR IMAGELOADWEBHOOKDTO
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
namespace backend.Application.Files.UploadFile;

public class UploadFileCommandHandler(
    IN8nService n8nService
) : IRequestHandler<UploadFileCommand, Result<ImageUploadResponseDto>>
{
    private readonly IN8nService _n8nService = n8nService; // ADD NEW SERVICE FIELD

    public async Task<Result<ImageUploadResponseDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // For image uploads, we use the n8n webhook directly.
        // File type and size validations are now handled by n8n webhook or client-side.

        // 1. Construct ImageUploadWebhookDto
        var imageUploadDto = new ImageUploadWebhookDto
        {
            ImageData = request.ImageData,
            FileName = request.FileName,
            Cloud = request.Cloud,
            Folder = request.Folder,
            ContentType = request.ContentType // Pass content type
        };

        var n8nUploadResult = await _n8nService.CallImageUploadWebhookAsync(imageUploadDto, cancellationToken);
        if (!n8nUploadResult.IsSuccess)
        {
            return Result<ImageUploadResponseDto>.Failure(n8nUploadResult.Error ?? ErrorMessages.FileUploadFailed, n8nUploadResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (n8nUploadResult.Value == null)
        {
            return Result<ImageUploadResponseDto>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
        }

        var uploadedImageUrl = n8nUploadResult.Value.Url; // Now this is safe

        if (string.IsNullOrEmpty(uploadedImageUrl))
        {
            return Result<ImageUploadResponseDto>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
        }

        return Result<ImageUploadResponseDto>.Success(n8nUploadResult.Value);
    }

    // Removed the SanitizeFileName method as it's no longer used.
}
