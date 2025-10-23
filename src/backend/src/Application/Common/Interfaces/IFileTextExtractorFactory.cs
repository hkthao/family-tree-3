namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà máy tạo các trình trích xuất văn bản từ tệp.
/// </summary>
public interface IFileTextExtractorFactory
{
    /// <summary>
    /// Lấy một thể hiện của IFileTextExtractor dựa trên phần mở rộng của tệp.
    /// </summary>
    /// <param name="fileExtension">Phần mở rộng của tệp (ví dụ: ".pdf", ".txt").</param>
    /// <returns>Một thể hiện của IFileTextExtractor.</returns>
    IFileTextExtractor GetExtractor(string fileExtension);
}
