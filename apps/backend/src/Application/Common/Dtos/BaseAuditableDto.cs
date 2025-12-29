namespace backend.Application.Common.Dtos;

/// <summary>
/// Lớp cơ sở trừu tượng cho các DTO có thông tin kiểm toán (audit).
/// </summary>
public abstract class BaseAuditableDto
{
    /// <summary>
    /// Thời gian tạo bản ghi.
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// Người tạo bản ghi.
    /// </summary>
    public string? CreatedBy { get; set; }
    /// <summary>
    /// Thời gian chỉnh sửa gần nhất của bản ghi.
    /// </summary>
    public DateTime? LastModified { get; set; }
    /// <summary>
    /// Người chỉnh sửa bản ghi gần nhất.
    /// </summary>
    public string? LastModifiedBy { get; set; }
    public bool IsPrivate { get; set; } = false; // Flag to indicate if some properties were hidden due to privacy
}
