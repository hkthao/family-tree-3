namespace backend.Application.MemberFaces.Common;

public class MemberFaceDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string FaceId { get; set; } = null!;
    public BoundingBoxDto BoundingBox { get; set; } = new BoundingBoxDto();
    public double Confidence { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public List<double> Embedding { get; set; } = new List<double>();
    public string? Emotion { get; set; }
    public double? EmotionConfidence { get; set; }
    public bool IsVectorDbSynced { get; set; }
    public string? VectorDbId { get; set; }

    // Enriched data
    public string? MemberName { get; set; }
    public string? MemberGender { get; set; } // NEW
    public string? MemberAvatarUrl { get; set; } // NEW
    public Guid? FamilyId { get; set; }
    public string? FamilyName { get; set; }
    public string? FamilyAvatarUrl { get; set; }
}
