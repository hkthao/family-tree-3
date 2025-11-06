using System.Text.Json.Serialization;

namespace backend.Application.Common.Models.AI;

/// <summary>
/// Đại diện cho cấu trúc phản hồi từ API chat của Ollama.
/// </summary>
public class OllamaChatResponse
{
    /// <summary>
    /// Đối tượng tin nhắn chứa phản hồi từ AI.
    /// </summary>
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = null!;

    // Các thuộc tính khác từ phản hồi của Ollama có thể được thêm vào đây nếu cần.
}
