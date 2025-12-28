using backend.Application.MemberFaces.Common;

namespace backend.Application.MemberFaces.Commands.ImportMemberFaces;

public record ImportMemberFaceItemDto
{
    public Guid? MemberId { get; set; } // Có thể null nếu khớp theo tên/tiêu chí khác trong quá trình nhập
    public string FaceId { get; set; } = null!;
    public BoundingBoxDto BoundingBox { get; set; } = new BoundingBoxDto();
    public double Confidence { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public List<double> Embedding { get; set; } = new List<double>();
    public string? Emotion { get; set; }
    public double? EmotionConfidence { get; set; }
}
