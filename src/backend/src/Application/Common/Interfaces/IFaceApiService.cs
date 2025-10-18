using backend.Application.Common.Models;
using backend.Application.Faces.Commands;

namespace backend.Application.Common.Interfaces;

public interface IFaceApiService
{
    Task<Result<List<float>>> GetFaceEmbeddingAsync(string base64Image, CancellationToken cancellationToken);
    Task<List<FaceDetectionResultDto>> DetectFacesAsync(byte[] imageBytes, string contentType, bool returnCrop);
    // Add other face-related API methods here (e.g., CompareFaces)
}
