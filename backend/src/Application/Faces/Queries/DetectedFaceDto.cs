
using FamilyTree.Application.Faces.Commands;

namespace FamilyTree.Application.Faces.Queries;
public class DetectedFaceDto
{
    public string Id { get; set; } = null!;
    public BoundingBoxDto BoundingBox { get; set; } = null!;
    public float Confidence { get; set; }
    public string? Thumbnail { get; set; } // Base64 encoded image
    public Guid? MemberId { get; set; } // Nullable, if not yet labeled
    public string? MemberName { get; set; } // For display
}