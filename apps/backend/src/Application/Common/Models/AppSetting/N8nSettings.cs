
namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ n8n.
/// </summary>
public class N8nSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "N8nSettings";
    /// <summary>
    /// URL của webhook để kích hoạt quy trình chat AI trên n8n.
    /// </summary>
    public string ChatWebhookUrl { get; set; } = null!;
    /// <summary>
    /// URL của webhook để kích hoạt quy trình embedding trên n8n.
    /// </summary>
    public string EmbeddingWebhookUrl { get; set; } = null!;
}
