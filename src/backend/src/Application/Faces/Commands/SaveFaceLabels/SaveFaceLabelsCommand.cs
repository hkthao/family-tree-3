using backend.Application.Common.Models;
using backend.Application.Faces.Queries; // Ensure DetectedFaceDto is accessible

namespace backend.Application.Faces.Commands.SaveFaceLabels;

public record SaveFaceLabelsCommand : IRequest<Result<Unit>>
{
    public List<DetectedFaceDto> FaceLabels { get; init; } = new List<DetectedFaceDto>();
    public Guid ImageId { get; init; } // Assuming an ImageId to associate faces with the original image
}
