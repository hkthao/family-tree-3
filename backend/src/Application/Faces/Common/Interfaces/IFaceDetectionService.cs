
using FamilyTree.Application.Faces.Commands;

namespace FamilyTree.Application.Faces.Common.Interfaces;
public interface IFaceDetectionService
{
    Task<List<FaceDetectionResultDto>> DetectFacesAsync(byte[] imageBytes, string contentType, bool returnCrop);
}