namespace FamilyTree.Application.Faces.Commands.LabelFace;
public class LabelFaceCommand : IRequest<Unit>
{
    public Guid MemberId { get; set; } // ID of the member to associate with
    public string FaceId { get; set; } = null!; // ID from the Python service response
    public BoundingBoxDto BoundingBox { get; set; } = null!;
    public float Confidence { get; set; }
    public string? Thumbnail { get; set; } // Base64 encoded image
}
