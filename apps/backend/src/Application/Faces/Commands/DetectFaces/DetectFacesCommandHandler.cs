using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Application.Files.UploadFile; // NEW
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, ILogger<DetectFacesCommandHandler> logger, IMediator mediator) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;
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
            string uploadFolder = UploadConstants.TemporaryUploadsFolder;
            string resizeFolder = UploadConstants.TemporaryUploadsFolder;

            if (request.ImageBytes != null && request.ImageBytes.Length > 0)
            {
                var originalUploadCommand = new UploadFileCommand // Ensure correct namespace
                {
                    ImageData = request.ImageBytes,
                    FileName = effectiveFileName,
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

                        var resizedUploadCommand = new UploadFileCommand // Ensure correct namespace
                        {
                            ImageData = resizedImageBytes,
                            FileName = resizedFileName,
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
                // Upload face thumbnail logic removed for performance.
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
                    Embedding = faceResult.Embedding?.ToList(),
                    MemberId = null,
                    MemberName = null,
                    FamilyId = null,
                    FamilyName = null,
                    BirthYear = null,
                    DeathYear = null,
                    Emotion = faceResult.Emotion,
                    EmotionConfidence = faceResult.EmotionConfidence,
                    Status = "unrecognized" // Default to unrecognized
                };

                if (detectedFaceDto.Embedding != null && detectedFaceDto.Embedding.Any())
                {
                    var searchFaceQuery = new SearchMemberFaceQuery
                    {
                        Vector = detectedFaceDto.Embedding,
                        Limit = 1, // We only need the best match for a detected face
                        Threshold = 0.7f // Default threshold for face matching
                    };

                    var searchResult = await _mediator.Send(searchFaceQuery, cancellationToken);

                    if (searchResult.IsSuccess && searchResult.Value != null && searchResult.Value.Any())
                    {
                        var foundFace = searchResult.Value.First(); // Get the best match
                        detectedFaceDto.MemberId = foundFace.MemberId;
                        memberIdsToFetch.Add(foundFace.MemberId);
                        faceMemberMap[detectedFaceDto.Id] = foundFace.MemberId;
                    }
                    else if (!searchResult.IsSuccess)
                    {
                        _logger.LogError("Failed to search face embedding for FaceId {FaceId}: {Error}", detectedFaceDto.Id, searchResult.Error);
                    }
                    else
                    {
                        _logger.LogInformation("No matching face found for FaceId {FaceId}.", detectedFaceDto.Id);
                    }
                }
                // NEW: Set Status based on whether MemberId has a value
                detectedFaceDto.Status = detectedFaceDto.MemberId.HasValue ? "recognized" : "unrecognized";
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

}
