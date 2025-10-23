namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình liên quan đến dịch vụ trò chuyện AI.
/// </summary>
public class AIChatSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "AIChatSettings";
    /// <summary>
    /// Nhà cung cấp dịch vụ trò chuyện AI được sử dụng (ví dụ: Gemini, OpenAI).
    /// </summary>
    public string Provider { get; set; } = null!;
    /// <summary>
    /// Ngưỡng điểm số để lọc các kết quả liên quan từ kho vector.
    /// </summary>
    public int ScoreThreshold { get; set; }
    /// <summary>
    /// Cài đặt cụ thể cho nhà cung cấp Gemini.
    /// </summary>
    public GeminiSettings Gemini { get; set; } = new GeminiSettings();
    /// <summary>
    /// Cài đặt cụ thể cho nhà cung cấp OpenAI.
    /// </summary>
    public OpenAISettings OpenAI { get; set; } = new OpenAISettings();
    /// <summary>
    /// Cài đặt cụ thể cho nhà cung cấp cục bộ.
    /// </summary>
    public LocalSettings Local { get; set; } = new LocalSettings();
}
