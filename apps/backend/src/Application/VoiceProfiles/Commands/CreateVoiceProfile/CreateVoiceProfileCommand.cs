using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

namespace backend.Application.VoiceProfiles.Commands.CreateVoiceProfile;

/// <summary>
/// Command để tạo một hồ sơ giọng nói mới.
/// </summary>
public record CreateVoiceProfileCommand : IRequest<Result<VoiceProfileDto>>
{
    /// <summary>
    /// ID của thành viên sở hữu hồ sơ giọng nói này.
    /// </summary>
    public Guid MemberId { get; init; }

    /// <summary>
    /// Nhãn để phân biệt các hồ sơ giọng nói (ví dụ: "default", "story", "elder").
    /// </summary>
    public string Label { get; init; } = null!;

    /// <summary>
    /// URL đến file audio đã được tiền xử lý (merged.wav).
    /// </summary>
    public string AudioUrl { get; init; } = null!;

    /// <summary>
    /// Thời lượng của file audio đã tiền xử lý tính bằng giây.
    /// </summary>
    public double DurationSeconds { get; init; }

    /// <summary>
    /// Ngôn ngữ của hồ sơ giọng nói.
    /// </summary>
    public string Language { get; init; } = null!;

    /// <summary>
    /// Trạng thái đồng ý của thành viên cho phép tạo giọng nói từ hồ sơ này.
    /// </summary>
    public bool Consent { get; init; }
}
