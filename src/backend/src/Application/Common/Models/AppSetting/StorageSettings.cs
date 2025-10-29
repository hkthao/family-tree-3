namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ lưu trữ tệp.
/// </summary>
public class StorageSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "StorageSettings";
    /// <summary>
    /// Nhà cung cấp dịch vụ lưu trữ được sử dụng (ví dụ: Local, Cloudinary, S3).
    /// </summary>
    public string Provider { get; set; } = null!;
    /// <summary>
    /// URL cơ sở để truy cập các tệp đã lưu trữ.
    /// </summary>
    public string BaseUrl { get; set; } = null!;
    /// <summary>
    /// Kích thước tệp tối đa được phép tải lên (MB).
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 5; // Default to 5MB
    /// <summary>
    /// Cài đặt cụ thể cho dịch vụ lưu trữ cục bộ.
    /// </summary>
    public LocalStorageSettings Local { get; set; } = new LocalStorageSettings();
    /// <summary>
    /// Cài đặt cụ thể cho dịch vụ Cloudinary.
    /// </summary>
    public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();
    /// <summary>
    /// Cài đặt cụ thể cho dịch vụ Amazon S3.
    /// </summary>
    public S3Settings S3 { get; set; } = new S3Settings();
}
