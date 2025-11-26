using backend.Application.Common.Models;

namespace backend.Application.Faces.Commands.DetectFaces;
public class DetectFacesCommand : IRequest<Result<FaceDetectionResponseDto>>
{
    public byte[] ImageBytes { get; set; } = null!;
    public string FileName { get; set; } = string.Empty; // NEW PROPERTY
    public bool ReturnCrop { get; set; } = true;
    public string Cloud { get; set; } = "imgbb"; // Default to imgbb
    public string Folder { get; set; } = "family-tree-face-detection"; // Default folder
    public string ContentType { get; set; } = string.Empty;
}
