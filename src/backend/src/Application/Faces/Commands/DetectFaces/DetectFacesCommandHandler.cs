using System.Text.Json;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, IConfigProvider configProvider, ILogger<DetectFacesCommandHandler> logger, IN8nService n8nService) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly IConfigProvider _configProvider = configProvider;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;
    private readonly IN8nService _n8nService = n8nService;

    public async Task<Result<FaceDetectionResponseDto>> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var detectedFacesResult = await _faceApiService.DetectFacesAsync(request.ImageBytes, request.ContentType, request.ReturnCrop); // Changed

            // Generate a unique ImageId for this detection session
            var imageId = Guid.NewGuid();

            // Store the original image or a reference to it if needed for later processing
            // For now, we'll just return the detected faces with the generated ImageId

            var detectedFaceDtos = new List<DetectedFaceDto>();
            var memberIdsToFetch = new HashSet<Guid>();
            var faceMemberMap = new Dictionary<string, Guid>(); // Map faceId to memberId from n8n

            foreach (var faceResult in detectedFacesResult)
            {
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
                    Thumbnail = faceResult.Thumbnail,
                    Embedding = faceResult.Embedding?.ToList(),
                    MemberId = null,
                    MemberName = null,
                    FamilyId = null,
                    FamilyName = null,
                    BirthYear = null,
                    DeathYear = null
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
                            var n8nResponse = JsonSerializer.Deserialize<N8nWebhookResponse>(n8nResult.Value);

                            if (n8nResponse == null)
                            {
                                _logger.LogWarning("n8n webhook response deserialized to null for FaceId {FaceId}: {N8nResultValue}", detectedFaceDto.Id, n8nResult.Value);
                                continue;
                            }

                            if (n8nResponse.result == null || n8nResponse.result.points == null || !n8nResponse.result.points.Any())
                            {
                                _logger.LogWarning("n8n webhook response result or points are null or empty for FaceId {FaceId}: {N8nResultValue}", detectedFaceDto.Id, n8nResult.Value);
                                continue;
                            }

                            var firstPoint = n8nResponse.result.points.First();
                            if (firstPoint.payload == null || string.IsNullOrEmpty(firstPoint.payload.memberId))
                            {
                                _logger.LogWarning("n8n webhook response payload or memberId is null or empty for FaceId {FaceId}: {N8nResultValue}", detectedFaceDto.Id, n8nResult.Value);
                                continue;
                            }

                            var memberIdFromN8n = Guid.Parse(firstPoint.payload.memberId);
                            detectedFaceDto.MemberId = memberIdFromN8n;
                            memberIdsToFetch.Add(memberIdFromN8n);
                            faceMemberMap[detectedFaceDto.Id] = memberIdFromN8n;
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize n8n result for FaceId {FaceId}: {N8nResultValue}", detectedFaceDto.Id, n8nResult.Value);
                        }
                        catch (FormatException ex)
                        {
                            _logger.LogError(ex, "Failed to parse MemberId from n8n result for FaceId {FaceId}: {N8nResultValue}", detectedFaceDto.Id, n8nResult.Value);
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
                DetectedFaces = detectedFaceDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during face detection.");
            return Result<FaceDetectionResponseDto>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message));
        }
    }

    // Internal classes for n8n webhook response deserialization
    private class N8nWebhookResponse
    {
        public ResultData? result { get; set; }
        public string? status { get; set; }
        public double time { get; set; }
    }

    private class ResultData
    {
        public List<Point>? points { get; set; }
    }

    private class Point
    {
        public string? id { get; set; }
        public int version { get; set; }
        public double score { get; set; }
        public Payload? payload { get; set; }
    }

    private class Payload
    {
        public string? memberId { get; set; }
        public string? actionType { get; set; }
        public string? entityType { get; set; }
        public string? description { get; set; }
    }
}
