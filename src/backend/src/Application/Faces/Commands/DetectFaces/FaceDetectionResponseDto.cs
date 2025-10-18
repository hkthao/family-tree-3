using backend.Application.Faces.Queries;

namespace backend.Application.Faces.Commands.DetectFaces;

public class FaceDetectionResponseDto
{
    public Guid ImageId { get; set; }
    public List<DetectedFaceDto> DetectedFaces { get; set; } = new List<DetectedFaceDto>();
}
