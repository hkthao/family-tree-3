using System.Text.RegularExpressions; // NEW USING FOR REGEX
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Application.Files.UploadFile; // NEW
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, ILogger<DetectFacesCommandHandler> logger, IN8nService n8nService, IMediator mediator) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;
    private readonly IN8nService _n8nService = n8nService;
    private readonly IMediator _mediator = mediator; // NEW

    public async Task<Result<FaceDetectionResponseDto>> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Ensure fileName is valid for the webhook
            string effectiveFileName = string.IsNullOrWhiteSpace(request.FileName)
                ? $"uploaded_image_{Guid.NewGuid()}{Path.GetExtension(request.ContentType)}"
                : request.FileName;

            // 1. Upload original image to n8n webhook to get a public URL
            string? originalImageUrl = null;
            byte[]? imageBytesToAnalyze = request.ImageBytes;
            string? resizedImageUrl = null;
            string cloud = "cloudinary";
            string uploadFolder = "temp/uploads";
            string faceFolder = "temp/faces";
            string resizeFolder = "temp/512x512";

            if (request.ImageBytes != null && request.ImageBytes.Length > 0)
            {
                var originalUploadCommand = new Files.UploadFile.UploadFileCommand // Ensure correct namespace
                {
                    ImageData = request.ImageBytes,
                    FileName = effectiveFileName,
                    Cloud = cloud,
                    Folder = uploadFolder,
                    ContentType = request.ContentType
                };

                var originalUploadResult = await _mediator.Send(originalUploadCommand, cancellationToken);

                if (originalUploadResult.IsSuccess && originalUploadResult.Value != null)
                {
                    originalImageUrl = originalUploadResult.Value.Url;
                }
                else
                {
                    return Result<FaceDetectionResponseDto>.Failure(originalUploadResult.Error ?? ErrorMessages.FileUploadFailed);
                }

                // If resizing is requested, resize the image and upload it
                if (request.ResizeImageForAnalysis)
                {
                    try
                    {
                        var resizedImageBytes = await _faceApiService.ResizeImageAsync(request.ImageBytes, request.ContentType, 512); // Resize to 512px width, auto height
                        var resizedFileName = $"resized_{effectiveFileName}";

                        var resizedUploadCommand = new Files.UploadFile.UploadFileCommand // Ensure correct namespace
                        {
                            ImageData = resizedImageBytes,
                            FileName = resizedFileName,
                            Cloud = cloud,
                            Folder = resizeFolder,
                            ContentType = request.ContentType
                        };

                        var resizedUploadResult = await _mediator.Send(resizedUploadCommand, cancellationToken);

                        if (resizedUploadResult.IsSuccess && resizedUploadResult.Value != null)
                        {
                            resizedImageUrl = resizedUploadResult.Value.Url;
                            imageBytesToAnalyze = resizedImageBytes; // Use resized image for detection
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload resized image to n8n: {Error}", resizedUploadResult.Error);
                            // Continue with original image if resizing upload fails, but log the error.
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to resize or upload image for analysis.");
                        // Continue with original image if resizing fails, but log the error.
                    }
                }
            }

            var detectedFacesResult = await _faceApiService.DetectFacesAsync(request.ImageBytes!, request.ContentType, request.ReturnCrop);

            // Generate a unique ImageId for this detection session
            var imageId = Guid.NewGuid();

            var detectedFaceDtos = new List<DetectedFaceDto>();
            var memberIdsToFetch = new HashSet<Guid>();
            var faceMemberMap = new Dictionary<string, Guid>(); // Map faceId to memberId from n8n

            foreach (var faceResult in detectedFacesResult)
            {
                // Upload face thumbnail to n8n to get a public URL
                string? thumbnailUrl = null;
                if (!string.IsNullOrEmpty(faceResult.Thumbnail))
                {
                    try
                    {
                        var thumbnailBytes = ConvertBase64ToBytes(faceResult.Thumbnail);
                        var thumbnailFileName = $"{faceResult.Id}_thumbnail.jpeg"; // Use face ID for unique name

                        var thumbnailUploadCommand = new UploadFileCommand // Ensure correct namespace
                        {
                            ImageData = thumbnailBytes,
                            FileName = thumbnailFileName,
                            Cloud = cloud,
                            Folder = faceFolder,
                            ContentType = "image/jpeg" // Assuming thumbnail is always jpeg
                        };

                        var thumbnailUploadResult = await _mediator.Send(thumbnailUploadCommand, cancellationToken);

                        if (thumbnailUploadResult.IsSuccess && thumbnailUploadResult.Value != null)
                        {
                            thumbnailUrl = thumbnailUploadResult.Value.Url;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload face thumbnail to n8n: {Error}", thumbnailUploadResult.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to convert or upload base64 thumbnail for face {FaceId}.", faceResult.Id);
                    }
                }

                var detectedFaceDto = new DetectedFaceDto
                {
                    Id = Guid.NewGuid().ToString(), // Generate a unique ID for each detected face
                    BoundingBox = new BoundingBoxDto
                    {
                        X = faceResult.BoundingBox.X,
                        Y = faceResult.BoundingBox.Y,
                        Width = faceResult.BoundingBox.Width,
                        Height = faceResult.BoundingBox.Height
                    },
                    Confidence = faceResult.Confidence,
                    Thumbnail = faceResult.Thumbnail, // Assign the original base64 thumbnail
                    ThumbnailUrl = thumbnailUrl, // Use the public URL
                    Embedding = faceResult.Embedding?.ToList(),
                    MemberId = null,
                    MemberName = null,
                    FamilyId = null,
                    FamilyName = null,
                    BirthYear = null,
                    DeathYear = null,
                    Emotion = faceResult.Emotion,
                    EmotionConfidence = faceResult.EmotionConfidence
                };

                if (detectedFaceDto.Embedding != null && detectedFaceDto.Embedding.Any())
                {
                    var embeddingDto = new EmbeddingWebhookDto
                    {
                        EntityType = "Face",
                        EntityId = detectedFaceDto.Id,
                        ActionType = "SearchFaceEmbedding",
                        EntityData = new { Embedding = detectedFaceDto.Embedding },
                        Description = $"Search face embedding for FaceId {detectedFaceDto.Id}"
                    };

                    var n8nResult = await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);

                    if (n8nResult.IsSuccess && !string.IsNullOrEmpty(n8nResult.Value))
                    {
                        try
                        {
                            var memberIdFromN8n = Guid.Parse(n8nResult.Value);
                            detectedFaceDto.MemberId = memberIdFromN8n;
                            memberIdsToFetch.Add(memberIdFromN8n);
                            faceMemberMap[detectedFaceDto.Id] = memberIdFromN8n;
                        }
                        catch (FormatException ex)
                        {
                            _logger.LogError(ex, "Failed to parse MemberId from n8n result: {N8nResultValue}", n8nResult.Value);
                        }
                    }
                    else if (!n8nResult.IsSuccess)
                    {
                        _logger.LogError("Failed to search face embedding for FaceId {FaceId} in n8n: {Error}", detectedFaceDto.Id, n8nResult.Error);
                    }
                }
                detectedFaceDtos.Add(detectedFaceDto);
            }

            // Fetch member details for all unique member IDs found
            if (memberIdsToFetch.Any())
            {
                var members = await _context.Members
                    .Where(m => memberIdsToFetch.Contains(m.Id))
                    .Include(m => m.Family)
                    .ToListAsync(cancellationToken);

                foreach (var faceDto in detectedFaceDtos)
                {
                    if (faceDto.MemberId.HasValue)
                    {
                        var member = members.FirstOrDefault(m => m.Id == faceDto.MemberId.Value);
                        if (member != null)
                        {
                            faceDto.MemberName = member.FullName;
                            faceDto.FamilyId = member.FamilyId;
                            faceDto.FamilyName = member.Family?.Name;
                            faceDto.BirthYear = member.DateOfBirth?.Year;
                            faceDto.DeathYear = member.DateOfDeath?.Year;
                        }
                    }
                }
            }

            return Result<FaceDetectionResponseDto>.Success(new FaceDetectionResponseDto
            {
                ImageId = imageId,
                OriginalImageUrl = originalImageUrl, // Populate the OriginalImageUrl
                ResizedImageUrl = resizedImageUrl, // Populate the ResizedImageUrl
                DetectedFaces = detectedFaceDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during face detection.");
            return Result<FaceDetectionResponseDto>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message));
        }
    }

    private byte[] ConvertBase64ToBytes(string base64String)
    {
        // Remove "data:image/jpeg;base64," or "data:image/png;base64," prefix if present
        var base64Data = Regex.Match(base64String, @"data:image/(?<type>.+?);base64,(?<data>.+)").Groups["data"].Value;
        if (string.IsNullOrEmpty(base64Data))
        {
            base64Data = base64String; // Fallback if no prefix or not an image data URI
        }
        return Convert.FromBase64String(base64Data);
    }
}
