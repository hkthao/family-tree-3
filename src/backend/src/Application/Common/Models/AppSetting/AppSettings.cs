namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt ứng dụng tổng thể, được ánh xạ từ tệp cấu hình.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Cài đặt cho dịch vụ trò chuyện AI.
    /// </summary>
    public AIChatSettings AIChatSettings { get; set; } = new AIChatSettings();
    /// <summary>
    /// Cài đặt cho dịch vụ tạo embedding.
    /// </summary>
    public EmbeddingSettings EmbeddingSettings { get; set; } = new EmbeddingSettings();
    /// <summary>
    /// Cài đặt cho kho lưu trữ vector.
    /// </summary>
    public VectorStoreSettings VectorStoreSettings { get; set; } = new VectorStoreSettings();
    /// <summary>
    /// Chuỗi kết nối cơ sở dữ liệu.
    /// </summary>
    public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    /// <summary>
    /// Cài đặt cho JSON Web Token (JWT).
    /// </summary>
    public JwtSettings JwtSettings { get; set; } = new JwtSettings();
    /// <summary>
    /// Cài đặt cho dịch vụ lưu trữ tệp.
    /// </summary>
    public StorageSettings StorageSettings { get; set; } = new StorageSettings();
    /// <summary>
    /// Cài đặt cho dịch vụ nhận diện khuôn mặt.
    /// </summary>
    public FaceDetectionSettings FaceDetectionService { get; set; } = new FaceDetectionSettings();
    /// <summary>
    /// Cài đặt cho dịch vụ thông báo.
    /// </summary>
    public NotificationSettings NotificationSettings { get; set; } = new NotificationSettings();
    /// <summary>
    /// Các nguồn gốc (origins) được phép cho CORS (Cross-Origin Resource Sharing).
    /// </summary>
    public string CORS_ORIGINS { get; set; } = null!;
    /// <summary>
    /// Cho biết có sử dụng cơ sở dữ liệu trong bộ nhớ hay không.
    /// </summary>
    public bool UseInMemoryDatabase { get; set; }
}
































