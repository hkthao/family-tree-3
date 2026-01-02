using backend.Domain.Common;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

/// <summary>
/// Đại diện cho một hồ sơ giọng nói của một thành viên.
/// Mỗi thành viên có thể có nhiều hồ sơ giọng nói, mỗi hồ sơ là một 'kiểu giọng' ổn định.
/// </summary>
public class VoiceProfile : BaseAuditableEntity
{
    /// <summary>
    /// ID của thành viên sở hữu hồ sơ giọng nói này.
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// Nhãn để phân biệt các hồ sơ giọng nói (ví dụ: "default", "story", "elder").
    /// </summary>
    public string Label { get; private set; } = null!;

    /// <summary>
    /// URL đến file audio đã được tiền xử lý (merged.wav).
    /// </summary>
    public string AudioUrl { get; private set; } = null!;

    /// <summary>
    /// Thời lượng của file audio đã tiền xử lý tính bằng giây.
    /// </summary>
    public double DurationSeconds { get; private set; }

    /// <summary>
    /// Ngôn ngữ của hồ sơ giọng nói.
    /// </summary>
    public string Language { get; private set; } = null!;

    /// <summary>
    /// Trạng thái đồng ý của thành viên cho phép tạo giọng nói từ hồ sơ này.
    /// </summary>
    public bool Consent { get; private set; }

    /// <summary>
    /// Trạng thái của hồ sơ giọng nói (Active, Archived).
    /// </summary>
    public VoiceProfileStatus Status { get; private set; } = VoiceProfileStatus.Active;

    // Navigation property
    /// <summary>
    /// Thành viên sở hữu hồ sơ giọng nói này.
    /// </summary>
    public Member Member { get; private set; } = null!;

    // Collection for VoiceGenerations
    /// <summary>
    /// Lịch sử các lần tạo giọng nói từ hồ sơ này.
    /// </summary>
    public ICollection<VoiceGeneration> VoiceGenerations { get; private set; } = new List<VoiceGeneration>();

    private VoiceProfile() { } // Required for EF Core

    /// <summary>
    /// Khởi tạo một thể hiện mới của VoiceProfile.
    /// </summary>
    public VoiceProfile(Guid memberId, string label, string audioUrl, double durationSeconds, string language, bool consent)
    {
        MemberId = memberId;
        Label = label;
        AudioUrl = audioUrl;
        DurationSeconds = durationSeconds;
        Language = language;
        Consent = consent;
        Status = VoiceProfileStatus.Active;
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ giọng nói.
    /// </summary>
    public void Update(string label, string audioUrl, double durationSeconds, string language, bool consent, VoiceProfileStatus status)
    {
        Label = label;
        AudioUrl = audioUrl;
        DurationSeconds = durationSeconds;
        Language = language;
        Consent = consent;
        Status = status;
    }

    /// <summary>
    /// Cập nhật trạng thái đồng ý cho hồ sơ giọng nói.
    /// </summary>
    public void UpdateConsent(bool consent)
    {
        Consent = consent;
    }

    /// <summary>
    /// Đánh dấu hồ sơ giọng nói là đã lưu trữ.
    /// </summary>
    public void Archive()
    {
        Status = VoiceProfileStatus.Archived;
    }

    /// <summary>
    /// Kích hoạt lại hồ sơ giọng nói.
    /// </summary>
    public void Activate()
    {
        Status = VoiceProfileStatus.Active;
    }
}
