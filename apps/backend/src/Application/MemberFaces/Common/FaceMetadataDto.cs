namespace backend.Application.MemberFaces.Common;

public class FaceMetadataDto
{
    public string LocalDbId { get; set; } = null!;
    public string MemberId { get; set; } = null!;
    public string FamilyId { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public string OriginalImageUrl { get; set; } = null!;
    public string Emotion { get; set; } = "";
    public float EmotionConfidence { get; set; } = 0.0f;
}
