namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình liên quan đến dịch vụ tạo embedding.
/// </summary>
public class EmbeddingSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "EmbeddingSettings";
    /// <summary>
    /// Nhà cung cấp dịch vụ tạo embedding được sử dụng (ví dụ: OpenAI, Cohere).
    /// </summary>
    public string Provider { get; set; } = null!;
    /// <summary>
    /// Cài đặt cụ thể cho nhà cung cấp OpenAI.
    /// </summary>
    public OpenAISettings OpenAI { get; set; } = new OpenAISettings();
    /// <summary>
    /// Cài đặt cụ thể cho nhà cung cấp Cohere.
    /// </summary>
    public CohereSettings Cohere { get; set; } = new CohereSettings();
    /// <summary>
    /// Cài đặt cụ thể cho nhà cung cấp cục bộ.
    /// </summary>
    public LocalSettings Local { get; set; } = new LocalSettings();
}
