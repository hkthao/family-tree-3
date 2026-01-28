using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class MemberFace : BaseAuditableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!; // Navigation property



    public BoundingBox BoundingBox { get; set; } = new BoundingBox();
    public double Confidence { get; set; }

    public string? ThumbnailUrl { get; set; } // URL to the uploaded face thumbnail
    public string? OriginalImageUrl { get; set; } // URL to the original image where the face was detected

    public List<double> Embedding { get; set; } = new List<double>(); // Face embedding vector

    public string? Emotion { get; set; }
    public double EmotionConfidence { get; set; }

    public bool IsVectorDbSynced { get; set; }
    public string? VectorDbId { get; set; }

    // Phương thức để đánh dấu khuôn mặt đã được đồng bộ hóa với Vector DB
    public void MarkAsVectorDbSynced(string vectorDbId)
    {
        if (string.IsNullOrWhiteSpace(vectorDbId))
        {
            throw new ArgumentException("VectorDbId cannot be null or empty.", nameof(vectorDbId));
        }
        VectorDbId = vectorDbId;
        IsVectorDbSynced = true;
    }

}
