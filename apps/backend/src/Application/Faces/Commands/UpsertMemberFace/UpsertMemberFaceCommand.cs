using backend.Application.Common.Models;
using backend.Application.Faces.Common;

namespace backend.Application.Faces.Commands.UpsertMemberFace;

public record UpsertMemberFaceCommand : IRequest<Result<UpsertMemberFaceCommandResultDto>>
{
    public Guid MemberId { get; init; }
    public Guid FamilyId { get; init; } // NEW
    public string FaceId { get; init; } = null!; // ID from face detection service
    public BoundingBoxDto BoundingBox { get; init; } = null!;
    public double Confidence { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? OriginalImageUrl { get; init; }
    public List<double> Embedding { get; init; } = new List<double>();
    public string? Emotion { get; init; }
    public double EmotionConfidence { get; init; }
}

public record UpsertMemberFaceCommandResultDto
{
    public string VectorDbId { get; init; } = null!;
}
