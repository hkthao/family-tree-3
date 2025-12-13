using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Utils;
using backend.Application.Files.UploadFile;
using MediatR;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

public class ThumbnailUploadService : IThumbnailUploadService
{
    private readonly IMediator _mediator;
    private readonly ILogger<ThumbnailUploadService> _logger;

    public ThumbnailUploadService(IMediator mediator, ILogger<ThumbnailUploadService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<string>> UploadThumbnailAsync(string base64Thumbnail, Guid memberFamilyId, string faceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(base64Thumbnail))
        {
            return Result<string>.Failure("Base64 thumbnail string cannot be null or empty.");
        }

        try
        {
            var thumbnailBytes = ImageUtils.ConvertBase64ToBytes(base64Thumbnail);
            var thumbnailFileName = $"{faceId}_thumbnail.jpeg"; // Use FaceId for unique name
            string faceFolder = string.Format(UploadConstants.FamilyFaceFolder, memberFamilyId); // Use FamilyFaceFolder constant for folder path

            var thumbnailUploadCommand = new UploadFileCommand
            {
                ImageData = thumbnailBytes,
                FileName = thumbnailFileName,
                Folder = faceFolder,
                ContentType = "image/jpeg" // Assuming thumbnail is always jpeg
            };

            var thumbnailUploadResult = await _mediator.Send(thumbnailUploadCommand, cancellationToken);

            if (thumbnailUploadResult.IsSuccess && thumbnailUploadResult.Value != null)
            {
                return Result<string>.Success(thumbnailUploadResult.Value.Url);
            }
            else
            {
                _logger.LogWarning("Failed to upload face thumbnail from base64 for FaceId {FaceId}: {Error}", faceId, thumbnailUploadResult.Error);
                return Result<string>.Failure(thumbnailUploadResult.Error ?? ErrorMessages.FileUploadFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert or upload base64 thumbnail for face {FaceId}.", faceId);
            return Result<string>.Failure($"Failed to process thumbnail for face {faceId}: {ex.Message}");
        }
    }
}
