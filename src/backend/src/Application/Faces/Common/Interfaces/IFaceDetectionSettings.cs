using backend.Application.Faces.Commands;

namespace backend.Application.Faces.Common.Interfaces;

public interface IFaceDetectionSettings
{
    Task<List<FaceDetectionResultDto>> DetectFacesAsync(byte[] imageBytes, string contentType, bool returnCrop);
}
