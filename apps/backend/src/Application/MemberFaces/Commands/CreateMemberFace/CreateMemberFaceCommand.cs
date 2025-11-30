using backend.Application.Common.Models;
using backend.Application.Faces.Common; // For BoundingBoxDto
using MediatR;

namespace backend.Application.MemberFaces.Commands.CreateMemberFace;

public record CreateMemberFaceCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }
    public string FaceId { get; init; } = null!;
    public BoundingBoxDto BoundingBox { get; init; } = new BoundingBoxDto();
    public double Confidence { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? OriginalImageUrl { get; init; }
    public List<double> Embedding { get; init; } = new List<double>();
    public string? Emotion { get; init; }
    public double? EmotionConfidence { get; init; }
    public bool IsVectorDbSynced { get; init; }
}
