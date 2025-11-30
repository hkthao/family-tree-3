using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For BoundingBoxDto

namespace backend.Application.MemberFaces.Commands.UpdateMemberFace;

public record UpdateMemberFaceCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; } // MemberId should probably not be updated, but including for now
    public string FaceId { get; init; } = null!;
    public BoundingBoxDto BoundingBox { get; init; } = new BoundingBoxDto();
    public double Confidence { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? OriginalImageUrl { get; init; }
    public List<double> Embedding { get; init; } = new List<double>();
    public string? Emotion { get; init; }
    public double? EmotionConfidence { get; init; }
    public bool IsVectorDbSynced { get; init; }
    public string? VectorDbId { get; init; }
}
