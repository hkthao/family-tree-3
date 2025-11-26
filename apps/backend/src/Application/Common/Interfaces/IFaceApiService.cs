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
    
    /// <summary>
    /// Thay đổi kích thước hình ảnh.
    /// </summary>
    /// <param name="imageBytes">Mảng byte của hình ảnh.</param>
    /// <param name="contentType">Loại nội dung của hình ảnh (ví dụ: "image/jpeg").</param>
    /// <param name="width">Chiều rộng mong muốn.</param>
    /// <param name="height">Chiều cao mong muốn (nếu không cung cấp, tỷ lệ khung hình sẽ được giữ nguyên).</param>
    /// <returns>Mảng byte của hình ảnh đã thay đổi kích thước.</returns>
    Task<byte[]> ResizeImageAsync(byte[] imageBytes, string contentType, int width, int? height = null);
}
