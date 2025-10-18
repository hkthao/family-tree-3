using backend.Application.Common.Interfaces;
using backend.Application.Faces.Queries; // For DetectedFaceDto
using backend.Application.Faces.Common; // Added

namespace backend.Application.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandler : IRequestHandler<DetectFacesCommand, FaceDetectionResponseDto>
{
    private readonly IFaceApiService _faceApiService; // Changed from IFaceDetectionService
    private readonly IApplicationDbContext _context;

    public DetectFacesCommandHandler(IFaceApiService faceApiService, IApplicationDbContext context) // Changed constructor
    {
        _faceApiService = faceApiService; // Changed
        _context = context;
    }

    public async Task<FaceDetectionResponseDto> Handle(DetectFacesCommand request, CancellationToken cancellationToken)
    {
        var detectedFacesResult = await _faceApiService.DetectFacesAsync(request.ImageBytes, request.ContentType, request.ReturnCrop); // Changed

        // Generate a unique ImageId for this detection session
        var imageId = Guid.NewGuid();

        // Store the original image or a reference to it if needed for later processing
        // For now, we'll just return the detected faces with the generated ImageId

        var detectedFaceDtos = new List<DetectedFaceDto>();
        foreach (var faceResult in detectedFacesResult)
        {
            // Map FaceDetectionResultDto to DetectedFaceDto
            detectedFaceDtos.Add(new DetectedFaceDto
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
                MemberId = null, // Initially null
                MemberName = null,
                FamilyId = null,
                FamilyName = null,
                BirthYear = null,
                DeathYear = null
            });
        }

        return new FaceDetectionResponseDto
        {
            ImageId = imageId,
            DetectedFaces = detectedFaceDtos
        };
    }
}