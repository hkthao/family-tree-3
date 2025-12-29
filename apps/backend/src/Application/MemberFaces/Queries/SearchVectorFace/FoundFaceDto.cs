namespace backend.Application.MemberFaces.Queries.SearchVectorFace;

public class FoundFaceDto
{
    public Guid MemberFaceId { get; set; } // Local DB ID of the found face
    public Guid MemberId { get; set; }
    public string FaceId { get; set; } = null!; // ID from face detection service
    public string? MemberName { get; set; }
    public float Score { get; set; } // Similarity score
    public string? ThumbnailUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? Emotion { get; set; }
    public double EmotionConfidence { get; set; }
    public string? FamilyAvatarUrl { get; set; }
    public bool IsPrivate { get; set; } = false; // Flag to indicate if some properties were hidden due to privacy
}
