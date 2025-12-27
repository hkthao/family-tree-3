using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.Commands.DetectFaces;

public class FaceDetectionResponseDto
{
    public Guid ImageId { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? ResizedImageUrl { get; set; }
    public int? ImageWidth { get; set; }
    public int? ImageHeight { get; set; }
    public List<DetectedFaceDto> DetectedFaces { get; set; } = [];
}
