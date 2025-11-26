using System.Text.RegularExpressions;
using backend.Application.AI.DTOs; // NEW USING FOR IMAGELOADWEBHOOKDTO
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Files.UploadFile;

public class UploadFileCommandHandler(
    IN8nService n8nService, // INJECT NEW SERVICE
    IConfiguration configuration,
    IApplicationDbContext context,
    IDateTime dateTime
) : IRequestHandler<UploadFileCommand, Result<string>>
{
    private readonly IN8nService _n8nService = n8nService; // ADD NEW SERVICE FIELD
    private readonly IConfiguration _configuration = configuration;
    private readonly IApplicationDbContext _context = context;
    private readonly IDateTime _dateTime = dateTime;

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

        var uploadedImageResponse = n8nUploadResult.Value!.FirstOrDefault(); // Added null-forgiving operator
        var uploadedImageUrl = uploadedImageResponse?.Url;

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
