namespace backend.Application.MemberFaces.Common;

public class FaceSearchResultPayloadDto
{
    public Guid FamilyId { get; set; }
    public Guid MemberId { get; set; }
    public Guid FaceId { get; set; }
    public BoundingBoxDto? BoundingBox { get; set; }
    public float Confidence { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? Emotion { get; set; }
    public float? EmotionConfidence { get; set; }
}
