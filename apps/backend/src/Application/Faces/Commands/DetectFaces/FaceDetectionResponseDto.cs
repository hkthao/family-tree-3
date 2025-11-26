using backend.Application.Faces.Queries;

namespace backend.Application.Faces.Commands.DetectFaces;

public class FaceDetectionResponseDto
{
    public Guid ImageId { get; set; }
    public string? OriginalImageUrl { get; set; } // NEW PROPERTY
    public List<DetectedFaceDto> DetectedFaces { get; set; } = [];
}
