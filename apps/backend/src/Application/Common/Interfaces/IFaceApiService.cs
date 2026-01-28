using backend.Application.MemberFaces.Common; // NEW: Add this using for FaceDetectionResultDto

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
    /// Thêm một khuôn mặt mới vào dịch vụ nhận diện khuôn mặt cùng với siêu dữ liệu.
    /// </summary>
    /// <param name="imageBytes">Mảng byte của hình ảnh khuôn mặt.</param>
    /// <param name="contentType">Loại nội dung của hình ảnh.</param>
    /// <param name="metadata">Siêu dữ liệu liên quan đến khuôn mặt.</param>
    /// <returns>Một từ điển chứa thông tin kết quả từ dịch vụ.</returns>
    Task<Dictionary<string, object>> AddFaceWithMetadataAsync(byte[] imageBytes, string contentType, FaceMetadataDto metadata);

    /// <summary>
    /// Thêm một khuôn mặt mới vào dịch vụ nhận diện khuôn mặt bằng cách cung cấp vector nhúng và siêu dữ liệu.
    /// </summary>
    /// <param name="request">Đối tượng chứa vector nhúng và siêu dữ liệu.</param>
    /// <returns>Một từ điển chứa thông tin kết quả từ dịch vụ.</returns>
    Task<Dictionary<string, object>> AddFaceByVectorAsync(FaceAddVectorRequestDto request);

    /// <summary>
    /// Lấy danh sách các khuôn mặt thuộc về một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách các từ điển chứa thông tin về các khuôn mặt.</returns>
    Task<List<Dictionary<string, object>>> GetFacesByFamilyIdAsync(string familyId);

    /// <summary>
    /// Xóa một khuôn mặt khỏi dịch vụ nhận diện khuôn mặt.
    /// </summary>
    /// <param name="faceId">ID của khuôn mặt cần xóa.</param>
    /// <returns>Một từ điển chứa thông báo xác nhận.</returns>
    Task<Dictionary<string, string>> DeleteFaceByIdAsync(string faceId);

    /// <summary>
    /// Tìm kiếm các khuôn mặt tương tự trong dịch vụ nhận diện khuôn mặt.
    /// </summary>
    /// <param name="request">Đối tượng chứa thông tin yêu cầu tìm kiếm.</param>
    /// <returns>Danh sách các kết quả tìm kiếm khuôn mặt.</returns>
    Task<List<FaceSearchResultDto>> SearchFacesAsync(FaceSearchVectorRequestDto request);
}
