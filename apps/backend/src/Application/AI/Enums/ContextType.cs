namespace backend.Application.AI.Enums;

/// <summary>
/// Các loại ngữ cảnh mà một tin nhắn người dùng có thể thuộc về.
/// </summary>
public enum ContextType
{
    /// <summary>
    /// Ngữ cảnh không xác định được.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Ngữ cảnh liên quan đến hỏi đáp về cách sử dụng ứng dụng hoặc thông tin chung.
    /// </summary>
    QA = 1,

    /// <summary>
    /// Ngữ cảnh liên quan đến tra cứu thông tin gia đình (ví dụ: "Huỳnh Kim Thao là ai?").
    /// </summary>
    FamilyDataLookup = 2,

    /// <summary>
    /// Ngữ cảnh yêu cầu tạo dữ liệu JSON từ đoạn chat.
    /// </summary>
    DataGeneration = 3
}
