using backend.Domain.Common;
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class MemberFace : BaseAuditableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!; // Navigation property

    public string FaceId { get; set; } = string.Empty; // ID from face detection service

    public BoundingBox BoundingBox { get; set; } = new BoundingBox();
    public double Confidence { get; set; }

    public string? ThumbnailUrl { get; set; } // URL to the uploaded face thumbnail
    public string? OriginalImageUrl { get; set; } // URL to the original image where the face was detected

    public List<double> Embedding { get; set; } = new List<double>(); // Face embedding vector

    public string? Emotion { get; set; }
    public double EmotionConfidence { get; set; }

    /// <summary>
    /// Đánh dấu xem thông tin khuôn mặt đã được đồng bộ hóa với cơ sở dữ liệu vector hay chưa.
    /// </summary>
    public bool IsVectorDbSynced { get; set; } = false;
}
