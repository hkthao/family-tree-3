using backend.Application.Faces.Common;

namespace backend.Application.Faces.Queries.SearchMemberFace;

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
}
