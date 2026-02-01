using backend.Application.Common.Models;

namespace backend.Application.MemberFaces.Commands.DetectFaces;

public class DetectFacesCommand : IRequest<Result<FaceDetectionResponseDto>>
{
    public byte[] ImageBytes { get; set; } = null!;
    public string? ImageUrl { get; set; } // NEW: URL to the image, if not provided as bytes
    public string FileName { get; set; } = string.Empty;
    public bool ReturnCrop { get; set; } = true;
    public string ContentType { get; set; } = string.Empty;
    public bool ResizeImageForAnalysis { get; set; } = false;
    public Guid FamilyId { get; set; }
}
