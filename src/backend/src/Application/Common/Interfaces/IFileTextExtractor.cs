namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho trình trích xuất văn bản từ tệp.
/// </summary>
public interface IFileTextExtractor
{
    /// <summary>
    /// Trích xuất văn bản từ một luồng tệp.
    /// </summary>
    /// <param name="fileStream">Luồng dữ liệu của tệp.</param>
    /// <returns>Nội dung văn bản được trích xuất từ tệp.</returns>
    Task<string> ExtractTextAsync(Stream fileStream);
}
