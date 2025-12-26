using backend.Application.AI.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.AI.Chat;

/// <summary>
/// Lệnh để bắt đầu một cuộc trò chuyện với AI Assistant.
/// </summary>
public record ChatWithAssistantCommand : IRequest<Result<ChatResponse>>
{
    /// <summary>
    /// ID của gia đình liên quan đến yêu cầu trò chuyện.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// ID phiên trò chuyện.
    /// </summary>
    public string SessionId { get; init; } = null!;

    /// <summary>
    /// Tin nhắn từ người dùng.
    /// </summary>
    public string ChatInput { get; init; } = null!;

    /// <summary>
    /// Metadata bổ sung cho cuộc trò chuyện.
    /// </summary>
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Danh sách các tệp đính kèm, bao gồm URL và loại nội dung.
    /// </summary>
    public ICollection<ChatAttachmentDto>? Attachments { get; init; }

    /// <summary>
    /// Thông tin vị trí liên quan đến cuộc trò chuyện.
    /// </summary>
    public ChatLocation? Location { get; init; }
}

/// <summary>
/// Đại diện cho thông tin vị trí trong cuộc trò chuyện.
/// </summary>
public record ChatLocation
{
    /// <summary>
    /// Vĩ độ.
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    /// Kinh độ.
    /// </summary>
    public double Longitude { get; init; }

    /// <summary>
    /// Địa chỉ (tùy chọn).
    /// </summary>
    public string? Address { get; init; }
}
