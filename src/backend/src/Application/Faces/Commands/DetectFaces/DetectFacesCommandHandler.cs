using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Faces.Common;
using backend.Application.Faces.Queries;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context, IConfigProvider configProvider, ILogger<DetectFacesCommandHandler> logger) : IRequestHandler<DetectFacesCommand, Result<FaceDetectionResponseDto>>
{
    private readonly IFaceApiService _faceApiService = faceApiService;
    private readonly IApplicationDbContext _context = context;
    private readonly IConfigProvider _configProvider = configProvider;
    private readonly ILogger<DetectFacesCommandHandler> _logger = logger;

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
                detectedFaceDtos.Add(detectedFaceDto);
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
}
