namespace backend.Application.MemberFaces.Common; // Place it in a common location

public class FaceDetectionResultDto
{
    public string Id { get; set; } = null!;
    public BoundingBoxDto BoundingBox { get; set; } = null!;
    public float Confidence { get; set; }
    public string? Thumbnail { get; set; } // Base64 encoded image
    public List<double>? Embedding { get; set; }
}
