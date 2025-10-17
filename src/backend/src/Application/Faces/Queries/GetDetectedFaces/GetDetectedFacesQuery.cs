namespace backend.Application.Faces.Queries.GetDetectedFaces;
public class GetDetectedFacesQuery : IRequest<List<DetectedFaceDto>>
{
    public Guid ImageId { get; set; } // Assuming an ImageId for temporary storage
}
