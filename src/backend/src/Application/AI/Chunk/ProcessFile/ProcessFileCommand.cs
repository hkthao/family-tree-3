using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.AI.Chunk.ProcessFile;

/// <summary>
/// Đại diện cho một lệnh để xử lý một tệp, trích xuất các đoạn văn bản và tạo embedding.
/// </summary>
public class ProcessFileCommand : IRequest<Result<List<TextChunk>>>
{
    /// <summary>
    /// Luồng dữ liệu của tệp.
    /// </summary>
    public Stream FileStream { get; set; } = null!;
    /// <summary>
    /// Tên của tệp.
    /// </summary>
    public string FileName { get; set; } = null!;
    /// <summary>
    /// ID duy nhất của tệp.
    /// </summary>
    public string FileId { get; set; } = null!;
    /// <summary>
    /// ID của gia đình liên quan đến tệp.
    /// </summary>
    public string FamilyId { get; set; } = null!;
    /// <summary>
    /// Danh mục của tệp.
    /// </summary>
    public string Category { get; set; } = null!;
    /// <summary>
    /// Người tạo tệp. 
    /// </summary>
    public string CreatedBy { get; set; } = null!;
}
