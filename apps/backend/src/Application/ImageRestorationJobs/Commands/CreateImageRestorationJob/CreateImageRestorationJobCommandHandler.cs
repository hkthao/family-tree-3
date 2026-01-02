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

    public CreateImageRestorationJobCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUser currentUser,
        IDateTime dateTime,
        ILogger<CreateImageRestorationJobCommandHandler> logger,
        IImageRestorationService imageRestorationService,
        IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
        _imageRestorationService = imageRestorationService;
        _mediator = mediator;
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
                                imageDataToUpload = Convert.FromBase64String(base64Data);            }
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
        if (!string.IsNullOrEmpty(restorationResult.Value.RestoredUrl))
        {
            entity.MarkAsCompleted(restorationResult.Value.RestoredUrl); // Using domain method to mark as completed
        }
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ImageRestorationJobDto>(entity);
        return Result<ImageRestorationJobDto>.Success(dto);
    }
}
