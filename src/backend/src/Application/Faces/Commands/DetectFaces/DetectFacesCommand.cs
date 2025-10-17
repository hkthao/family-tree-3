namespace backend.Application.Faces.Commands.DetectFaces;
public class DetectFacesCommand : IRequest<List<FaceDetectionResultDto>>
{
    public byte[] ImageBytes { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public bool ReturnCrop { get; set; } = true;
}