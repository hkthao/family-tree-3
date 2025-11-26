using System.Text.RegularExpressions; // NEW USING FOR REGEX
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using Microsoft.Extensions.Logging;
using backend.Application.AI.DTOs; // NEW USING FOR IMAGEUPLOADWEBHOOKDTO
using backend.Application.Memories.DTOs; // For ImageUploadResponseDto
using System.Linq; // For FirstOrDefault

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, ILogger<DetectFacesCommandHandler> logger, IN8nService n8nService) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;
    private readonly IN8nService _n8nService = n8nService;

    public async Task<Result<FaceDetectionResponseDto>> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Upload original image to n8n webhook to get a public URL
            string? originalImageUrl = null;
            if (request.ImageBytes != null && request.ImageBytes.Length > 0)
            {
                var imageUploadDto = new ImageUploadWebhookDto
                {
                    ImageData = request.ImageBytes,
                    FileName = request.FileName,
                    Cloud = request.Cloud,
                    Folder = request.Folder
                };

                var n8nImageUploadResult = await _n8nService.CallImageUploadWebhookAsync(imageUploadDto, cancellationToken);

                if (n8nImageUploadResult.IsSuccess && n8nImageUploadResult.Value != null && n8nImageUploadResult.Value.Any())
                {
                    originalImageUrl = n8nImageUploadResult.Value.FirstOrDefault()?.Url;
                    if (string.IsNullOrEmpty(originalImageUrl))
                    {
                        _logger.LogWarning("n8n image upload returned success but no URL for original image.");
                        return Result<FaceDetectionResponseDto>.Failure(ErrorMessages.FileUploadNullUrl, ErrorSources.ExternalServiceError);
                    }
                }
                else if (!n8nImageUploadResult.IsSuccess)
                {
                    _logger.LogError("Failed to upload original image to n8n: {Error}", n8nImageUploadResult.Error);
                    return Result<FaceDetectionResponseDto>.Failure(n8nImageUploadResult.Error ?? ErrorMessages.FileUploadFailed, ErrorSources.ExternalServiceError);
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
                        var thumbnailUploadDto = new ImageUploadWebhookDto
                        {
                            ImageData = thumbnailBytes,
                            FileName = thumbnailFileName,
                            Cloud = request.Cloud,
                            Folder = request.Folder
                        };

                        var n8nThumbnailUploadResult = await _n8nService.CallImageUploadWebhookAsync(thumbnailUploadDto, cancellationToken);
                        if (n8nThumbnailUploadResult.IsSuccess && n8nThumbnailUploadResult.Value != null && n8nThumbnailUploadResult.Value.Any())
                        {
                            thumbnailUrl = n8nThumbnailUploadResult.Value.FirstOrDefault()?.Url;
                            if (string.IsNullOrEmpty(thumbnailUrl))
                            {
                                _logger.LogWarning("n8n image upload returned success but no URL for face thumbnail {FaceId}.", faceResult.Id);
                            }
                        }
                        else if (!n8nThumbnailUploadResult.IsSuccess)
                        {
                            _logger.LogError("Failed to upload face thumbnail {FaceId} to n8n: {Error}", faceResult.Id, n8nThumbnailUploadResult.Error);
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
                    Thumbnail = thumbnailUrl, // Use the public URL
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
