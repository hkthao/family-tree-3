using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia;
using backend.Application.ImageRestorationJobs.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public class CreateImageRestorationJobCommandHandler : IRequestHandler<CreateImageRestorationJobCommand, Result<ImageRestorationJobDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;
    private readonly ILogger<CreateImageRestorationJobCommandHandler> _logger;
    private readonly IImageRestorationService _imageRestorationService;
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory; // Added

    public CreateImageRestorationJobCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUser currentUser,
        IDateTime dateTime,
        ILogger<CreateImageRestorationJobCommandHandler> logger,
        IImageRestorationService imageRestorationService,
        IMediator mediator,
        IHttpClientFactory httpClientFactory) // Added
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
        _imageRestorationService = imageRestorationService;
        _mediator = mediator;
        _httpClientFactory = httpClientFactory; // Added
    }

    public async Task<Result<ImageRestorationJobDto>> Handle(CreateImageRestorationJobCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Current user is not authenticated. Cannot create image restoration job.");
            return Result<ImageRestorationJobDto>.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.ToString();

        // 1. Preprocess the image
        string imageUrlForRestoration;
        byte[] originalImageData = request.ImageData;

        using (var imageStream = new MemoryStream(originalImageData))
        {
            var preprocessResult = await _imageRestorationService.PreprocessImageAsync(
                imageStream,
                request.FileName,
                request.ContentType,
                cancellationToken
            );

            if (!preprocessResult.IsSuccess)
            {
                _logger.LogError("Image preprocessing failed: {ErrorMessage}", preprocessResult.Error);
                return Result<ImageRestorationJobDto>.Failure($"Image preprocessing failed: {preprocessResult.Error}");
            }

            var preprocessedImage = preprocessResult.Value;
            if (preprocessedImage == null)
            {
                _logger.LogError("Preprocessed image result value is null.");
                return Result<ImageRestorationJobDto>.Failure("Preprocessed image result is null.");
            }

            byte[] imageDataToUpload;

            if (preprocessedImage.IsResized)
            {
                _logger.LogInformation("Image was resized during preprocessing. Preparing resized image for upload.");
                string? base64Data = null;
                if (!string.IsNullOrEmpty(preprocessedImage.ProcessedImageBase64))
                {
                    var parts = preprocessedImage.ProcessedImageBase64.Split(',');
                    if (parts.Length == 2)
                    {
                        base64Data = parts[1];
                    }
                }

                if (string.IsNullOrEmpty(base64Data))
                {
                    _logger.LogError("Invalid base64 data received from preprocessed image.");
                    return Result<ImageRestorationJobDto>.Failure("Invalid base64 data from preprocessed image.");
                }
                imageDataToUpload = Convert.FromBase64String(base64Data);
            }
            else
            {
                _logger.LogInformation("Image was not resized during preprocessing. Preparing original image for upload.");
                imageDataToUpload = originalImageData;
            }

            // Upload the image using CreateFamilyMediaCommand
            var createMediaCommand = new CreateFamilyMediaCommand
            {
                FamilyId = request.FamilyId,
                File = imageDataToUpload,
                FileName = request.FileName,
                MediaType = MediaType.Image,
                Folder = string.Format(UploadConstants.ImagesFolder, request.FamilyId),
                Description = $"Image for restoration job {Guid.NewGuid()}"
            };

            var mediaUploadResult = await _mediator.Send(createMediaCommand, cancellationToken);

            if (!mediaUploadResult.IsSuccess)
            {
                _logger.LogError("Failed to upload image via CreateFamilyMediaCommand: {ErrorMessage}", mediaUploadResult.Error);
                return Result<ImageRestorationJobDto>.Failure($"Failed to upload image: {mediaUploadResult.Error}");
            }
            imageUrlForRestoration = mediaUploadResult.Value!.FilePath;
        }

        var entity = new ImageRestorationJob();
        entity.Initialize(imageUrlForRestoration, userId, request.FamilyId); // Using domain method to initialize

        entity.Created = _dateTime.Now;
        entity.CreatedBy = userId;
        entity.LastModified = _dateTime.Now;
        entity.LastModifiedBy = userId;

        _context.ImageRestorationJobs.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Start image restoration process
        _logger.LogInformation("Attempting to start image restoration for job {JobId} with image URL {ImageUrl}", entity.Id, entity.OriginalImageUrl);
        var restorationResult = await _imageRestorationService.StartRestorationAsync(entity.OriginalImageUrl, request.UseCodeformer, cancellationToken);

        if (!restorationResult.IsSuccess)
        {
            _logger.LogError("Failed to start image restoration for job {JobId}: {ErrorMessage}", entity.Id, restorationResult.Error);
            entity.MarkAsFailed(restorationResult.Error ?? string.Empty);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<ImageRestorationJobDto>.Failure($"Failed to start image restoration: {restorationResult.Error}");
        }

        entity.MarkAsProcessing(restorationResult.Value!.JobId.ToString());

        // If RestoredUrl is returned, download and upload it
        if (!string.IsNullOrEmpty(restorationResult.Value.RestoredUrl))
        {
            try
            {
                _logger.LogInformation("RestoredUrl received for job {JobId}. Downloading restored image from {RestoredUrl} for final upload.", entity.Id, restorationResult.Value.RestoredUrl);

                var httpClient = _httpClientFactory.CreateClient();
                var restoredImageData = await httpClient.GetByteArrayAsync(restorationResult.Value.RestoredUrl, cancellationToken);

                var finalCreateMediaCommand = new CreateFamilyMediaCommand
                {
                    FamilyId = request.FamilyId,
                    File = restoredImageData,
                    FileName = $"restored_{request.FileName}", // Use a distinct name
                    MediaType = MediaType.Image,
                    Folder = string.Format(UploadConstants.ImagesFolder, request.FamilyId),
                    Description = $"Restored image for job {entity.Id}"
                };

                var finalMediaUploadResult = await _mediator.Send(finalCreateMediaCommand, cancellationToken);

                if (!finalMediaUploadResult.IsSuccess)
                {
                    _logger.LogError("Failed to upload final restored image via CreateFamilyMediaCommand for job {JobId}: {ErrorMessage}", entity.Id, finalMediaUploadResult.Error);
                    entity.MarkAsFailed($"Failed to upload final restored image: {finalMediaUploadResult.Error}");
                    await _context.SaveChangesAsync(cancellationToken);
                    return Result<ImageRestorationJobDto>.Failure($"Failed to upload final restored image: {finalMediaUploadResult.Error}");
                }

                entity.MarkAsCompleted(finalMediaUploadResult.Value!.FilePath);
                _logger.LogInformation("Final restored image uploaded for job {JobId}. Final URL: {FinalUrl}", entity.Id, finalMediaUploadResult.Value.FilePath);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when downloading restored image for job {JobId} from {RestoredUrl}: {Message}", entity.Id, restorationResult.Value.RestoredUrl, ex.Message);
                entity.MarkAsFailed($"Failed to download restored image: {ex.Message}");
                await _context.SaveChangesAsync(cancellationToken);
                return Result<ImageRestorationJobDto>.Failure($"Failed to download restored image: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred when handling restored image for job {JobId}: {Message}", entity.Id, ex.Message);
                entity.MarkAsFailed($"Unexpected error handling restored image: {ex.Message}");
                await _context.SaveChangesAsync(cancellationToken);
                return Result<ImageRestorationJobDto>.Failure($"Unexpected error handling restored image: {ex.Message}");
            }
        }
        else
        {
            // If no RestoredUrl is provided by the service, mark the job as completed with the original image URL
            // or perhaps as failed if it implies a problem. For now, mark as completed with original.
            _logger.LogWarning("Image restoration service did not return a RestoredUrl for job {JobId}. Marking job as completed with original image URL.", entity.Id);
            entity.MarkAsCompleted(entity.OriginalImageUrl);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ImageRestorationJobDto>(entity);
        return Result<ImageRestorationJobDto>.Success(dto);
    }
}
