using backend.Application.Common.Models;
using backend.Application.MemberFaces.Common; // For BoundingBoxDto

namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public record CreateMemberFaceCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }

    public BoundingBoxDto BoundingBox { get; init; } = new BoundingBoxDto();
    public double Confidence { get; init; }
    public List<double> Embedding { get; init; } = [];
    public string? Emotion { get; init; }
    public double? EmotionConfidence { get; init; }
    public string? Thumbnail { get; init; } // NEW: Base64 encoded thumbnail
}
