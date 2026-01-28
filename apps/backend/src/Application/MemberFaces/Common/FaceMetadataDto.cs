namespace backend.Application.MemberFaces.Common;

public class FaceMetadataDto
{
    public string FamilyId { get; set; } = null!;
    public string MemberId { get; set; } = null!;
    public string FaceId { get; set; } = null!; // Corresponds to local MemberFace.Id
    public BoundingBoxDto BoundingBox { get; set; } = null!;
    public double Confidence { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? Emotion { get; set; } = "";
    public double EmotionConfidence { get; set; } = 0.0;
}
