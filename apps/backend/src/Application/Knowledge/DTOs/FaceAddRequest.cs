using backend.Domain.ValueObjects;

namespace backend.Application.Knowledge.DTOs;

public class FaceAddRequest
{
    public Guid FamilyId { get; set; }
    public Guid MemberId { get; set; }
    public Guid FaceId { get; set; }
    public BoundingBox? BoundingBox { get; set; }
    public double Confidence { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public List<double>? Embedding { get; set; }
    public string? Emotion { get; set; }
    public double EmotionConfidence { get; set; }
    public string? VectorDbId { get; set; }
    public bool IsVectorDbSynced { get; set; }
}
