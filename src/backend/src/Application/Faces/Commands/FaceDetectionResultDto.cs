using backend.Application.Faces.Common; 

namespace backend.Application.Faces.Commands;

public class FaceDetectionResultDto
{
    public string Id { get; set; } = null!;
    public BoundingBoxDto BoundingBox { get; set; } = null!;
    public float Confidence { get; set; }
    public string? Thumbnail { get; set; } // Base64 encoded image
    public float[]? Embedding { get; set; } // Changed from List<float>
}