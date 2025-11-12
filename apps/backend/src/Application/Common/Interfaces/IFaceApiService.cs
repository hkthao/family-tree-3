using backend.Application.Faces.Commands;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ API nhận diện khuôn mặt.
/// </summary>
public interface IFaceApiService
{
    /// <summary>
    /// Phát hiện các khuôn mặt trong một hình ảnh.
    /// </summary>
    /// <param name="imageBytes">Mảng byte của hình ảnh.</param>
    /// <param name="contentType">Loại nội dung của hình ảnh (ví dụ: "image/jpeg").</param>
    /// <param name="returnCrop">Cho biết có trả về ảnh cắt của khuôn mặt hay không.</param>
    /// <returns>Danh sách các đối tượng FaceDetectionResultDto chứa thông tin về các khuôn mặt được phát hiện.</returns>
    Task<List<FaceDetectionResultDto>> DetectFacesAsync(byte[] imageBytes, string contentType, bool returnCrop);
    // Add other face-related API methods here (e.g., CompareFaces)
}
