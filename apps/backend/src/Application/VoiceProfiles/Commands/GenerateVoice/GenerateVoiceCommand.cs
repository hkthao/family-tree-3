using backend.Application.Common.Models;
using backend.Application.VoiceProfiles.Queries;

namespace backend.Application.VoiceProfiles.Commands.GenerateVoice;

/// <summary>
/// Command để tạo audio từ một hồ sơ giọng nói và văn bản.
/// </summary>
public record GenerateVoiceCommand : IRequest<Result<VoiceGenerationDto>>
{
    /// <summary>
    /// ID của hồ sơ giọng nói sẽ được sử dụng.
    /// </summary>
    public Guid VoiceProfileId { get; init; }

    /// <summary>
    /// Văn bản cần chuyển đổi thành giọng nói.
    /// </summary>
    public string Text { get; init; } = null!;
}
