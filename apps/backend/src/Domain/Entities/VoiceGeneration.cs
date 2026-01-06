namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho lịch sử mỗi lần tạo giọng nói từ một hồ sơ giọng nói cụ thể.
/// </summary>
public class VoiceGeneration : BaseAuditableEntity
{
    /// <summary>
    /// ID của hồ sơ giọng nói được sử dụng để tạo audio này.
    /// </summary>
    public Guid VoiceProfileId { get; private set; }

    /// <summary>
    /// Văn bản đã được sử dụng để tạo audio.
    /// </summary>
    public string Text { get; private set; } = null!;

    /// <summary>
    /// URL đến file audio đã được tạo.
    /// </summary>
    public string AudioUrl { get; private set; } = null!;

    /// <summary>
    /// Thời lượng của audio đã tạo tính bằng giây.
    /// </summary>
    public double Duration { get; private set; }

    // Navigation property
    /// <summary>
    /// Hồ sơ giọng nói đã tạo ra bản ghi này.
    /// </summary>
    public VoiceProfile VoiceProfile { get; private set; } = null!;

    private VoiceGeneration() { } // Required for EF Core

    /// <summary>
    /// Khởi tạo một thể hiện mới của VoiceGeneration.
    /// </summary>
    public VoiceGeneration(Guid voiceProfileId, string text, string audioUrl, double duration)
    {
        VoiceProfileId = voiceProfileId;
        Text = text;
        AudioUrl = audioUrl;
        Duration = duration;
    }
}
